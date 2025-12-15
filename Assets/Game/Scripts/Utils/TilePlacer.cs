#region _____________________________/ INFOS
//  AUTHOR : Rush Team
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using System;
using System.Collections.Generic;
using DG.Tweening;
using Rush.Game;
using UnityEngine;
using UnityEngine.EventSystems;

public class TilePlacer : MonoBehaviour
{
    #region _____________________________/ SINGLETON

    public static TilePlacer Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    #endregion

    #region _____________________________/ PREFABS

    [Header("Prefabs")]
    [SerializeField] private Transform _TileToSpawn;
    [SerializeField] private Transform _TilePreviewPrefab;

    [Header("Effects")]
    [SerializeField] private ParticleSystem _PlacementDustEffect;
    [SerializeField] private Vector3 _DustSpawnOffset = new(0f, 0.1f, 0f);
    #endregion

    #region _____________________________/ PHYSICS

    [Header("Physics")]
    [SerializeField] private float _RaycastDistance = 20f;
    private Vector3 _LastGroundNormal = Vector3.up;

    [SerializeField] private float _PreviewHoverDistance = 20f;
    [SerializeField] private float _GroundCheckRadius = 0.4f;
    [SerializeField] private float _PlacedTileHoverYOffset = 0.25f;
    [SerializeField] private float _GroundHitYOffset = 0.25f;
    [SerializeField] private LayerMask _GroundLayer, _UiLayer, _TilesLayer;

    #endregion

    #region _____________________________/ STATE
    private bool _HasGroundHit;
    private Vector3 _InstantiatePos;
    private Quaternion _TileRotation = Quaternion.identity;
    private readonly Dictionary<Transform, Transform> _PlacedTiles = new();
    private Transform _CurrentDisabledGround;
        private Transform _CurrentRaisedTile;

    public static Transform previewTile;
    public event Action OnTilePlaced;
    public bool HandlingTile { get; private set; }

    #endregion

    #region _____________________________| UNITY

    private void Start()
    {
        _InstantiatePos = new Vector3(1000, 1000, 1000);

        if (_TilePreviewPrefab != null)
            InstantiatePreviewTile(_TilePreviewPrefab, _InstantiatePos);
    }

    private void Update()
    {

        if (Input.GetMouseButtonDown(1))
        {
            if (TryRemoveTileUnderCursor()) return;

            if (!HandlingTile) return;
            RestoreCurrentHitGround();

            if (previewTile != null)
            {
                Destroy(previewTile.gameObject);
                previewTile = null;
            }

            HandlingTile = false;
            return;
        }

        if (!HandlingTile)
        {
            HandlePlacedTileHover();

            if (Input.GetMouseButtonUp(0))
                TryPickUpTileUnderCursor();
            
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject()) return;

        Camera lRaycastCamera = Manager_Camera.Instance?.GetActiveCameraOrMain() ?? Camera.main;

        if (lRaycastCamera == null)
            return;

        Ray lRay = lRaycastCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit lHitObject;
        _HasGroundHit = false;
        Vector3 lHoverPosition = lRay.GetPoint(_PreviewHoverDistance);
        
        if (previewTile != null)
        {
            if (TryGetPlacementHit(lRay, out lHitObject))
            {
                if (lHitObject.transform.gameObject.layer == 7) 
                {
                    return;
                }
                float lUpDot = Vector3.Dot(lHitObject.normal, Vector3.up);

                if (lUpDot > 0.9f)
                {
                    Vector3 lOffsetPoint = lHitObject.point + lHitObject.normal * 0.5f;
                    SetCurrentHitGround(lHitObject.transform);

                    previewTile.position = Vector3Int.RoundToInt(lOffsetPoint);
                    _HasGroundHit = true;
                }
                else
                {                    RestoreCurrentHitGround();

                    previewTile.position = _InstantiatePos;
                }
            }
            else
            {
                previewTile.position = lHoverPosition;
            }
        }

        if (_TileToSpawn != null && previewTile != null && _HasGroundHit && Input.GetMouseButtonUp(0))
        {

            Transform lNewTile = Instantiate(_TileToSpawn, previewTile.position, _TileRotation);
            Vector3 lLandingPosition = previewTile.position;

            PlayPlacementTween(lNewTile, lLandingPosition);
            Arrow lArrowComponent = lNewTile.GetComponentInChildren<Arrow>();

            if (lArrowComponent != null)
                lArrowComponent.PlayPlacementAnimation();
            if (_CurrentDisabledGround != null)
                _PlacedTiles[lNewTile] = _CurrentDisabledGround;
            DetachCurrentHitGround();
            Destroy(previewTile.gameObject);
            previewTile = null;
            HandlingTile = false;
            OnTilePlaced?.Invoke();
        }
    }

    #endregion

    #region _____________________________| SETUP

    public void SetTilePrefabs(Transform pTileToSpawn, Transform pPreviewPrefab, Tile.TileOrientations pOrientation)
    {
        _TileToSpawn = pTileToSpawn;
        _TilePreviewPrefab = pPreviewPrefab != null ? pPreviewPrefab : pTileToSpawn;
        _TileRotation = GetRotationFromOrientation(pOrientation);

        if (_TilePreviewPrefab == null) return;

        if (previewTile == null)
            InstantiatePreviewTile(_TilePreviewPrefab, _InstantiatePos);
        else
            SwitchPreviewTile(_TilePreviewPrefab);

        previewTile.rotation = _TileRotation;
    }

    private void InstantiatePreviewTile(Transform pPrefab, Vector3 pPosition) => previewTile = Instantiate(pPrefab, pPosition, _TileRotation);

    public void SwitchPreviewTile(Transform pPrefab)
    {
        Vector3 lCurrentPos = previewTile.position;

        Destroy(previewTile.gameObject);
        InstantiatePreviewTile(pPrefab, lCurrentPos);
    }

    private static Quaternion GetRotationFromOrientation(Tile.TileOrientations pOrientation)
    {
        return pOrientation switch
        {
            Tile.TileOrientations.Right => Quaternion.Euler(0f, 90f, 0f),
            Tile.TileOrientations.Left => Quaternion.Euler(0f, -90f, 0f),
            Tile.TileOrientations.Down => Quaternion.Euler(0f, 180f, 0f),
            _ => Quaternion.identity,
        };
    }

    #endregion

    #region _____________________________| PLACEMENT

    private bool TryGetPlacementHit(Ray pRay, out RaycastHit pHit)
    {
        var lHits = Physics.RaycastAll(pRay, _RaycastDistance, _GroundLayer | _TilesLayer);
        Array.Sort(lHits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var lHit in lHits)
        {
            if (previewTile != null && (lHit.transform == previewTile || lHit.transform.IsChildOf(previewTile)))
                continue;

            pHit = lHit;
            return true;
        }

        pHit = default;
        return false;
    }

    public void StartHandlingTile()
    {
        HandlingTile = true;
    }

    public void ClearSelection()
    {
        _TileToSpawn = null;
        _TilePreviewPrefab = null;

        if (previewTile != null)
            previewTile.position = _InstantiatePos;

                RestoreCurrentHitGround();
    }

    public void ResetPlacedTiles()
    {
        foreach (KeyValuePair<Transform, Transform> lPlacedTile in _PlacedTiles)
        {
            Transform lTile = lPlacedTile.Key;
            if (lTile == null) continue;

            Tile lTileComponent = lTile.GetComponentInChildren<Tile>();

            if (lTileComponent != null)
            {
                Tile.TileOrientations lOrientation = GetOrientationFromDirection(lTile.forward);
                UI_Btn_InventoryTile lInventoryTile = UI_Btn_InventoryTile.FindMatchingTile(lTileComponent.tileVariant, lOrientation);
                lInventoryTile?.AddTileBack();
            }

            Vector3 lTilePosition = lTile.position;
            Destroy(lTile.gameObject);
        }

        foreach (Transform lGround in _PlacedTiles.Values)
        {
            EnableChild(lGround);
        }

        _PlacedTiles.Clear();
        HandlingTile = false;

        ClearSelection();
    }

        private bool TryRemoveTileUnderCursor()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return false;

        Ray lRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(lRay, out RaycastHit lHit, _RaycastDistance, _TilesLayer))
            return false;

        if (previewTile != null && (lHit.transform == previewTile || lHit.transform.IsChildOf(previewTile)))
            return false;

        Tile lTile = lHit.transform.GetComponentInParent<Tile>();

        if (lTile == null)
            return false;
            
        if (lTile.tileVariant == Tile.TileVariants.Teleporter
            || lTile.tileVariant == Tile.TileVariants.Target
            || lTile.tileVariant == Tile.TileVariants.Spawner)
            return false;
        Tile.TileOrientations lOrientation = GetOrientationFromDirection(lTile.transform.forward);

        UI_Btn_InventoryTile lInventoryTile = UI_Btn_InventoryTile.FindMatchingTile(lTile.tileVariant, lOrientation);

        if (lInventoryTile != null)
            lInventoryTile.AddTileBack();
        if (_PlacedTiles.TryGetValue(lTile.transform, out Transform lGround))
        {
            EnableChild(lGround);
        }
        _PlacedTiles.Remove(lTile.transform);
        Destroy(lTile.gameObject);

        return true;
    }

    private void TryPickUpTileUnderCursor()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Ray lRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(lRay, out RaycastHit lHit, _RaycastDistance, _TilesLayer))
            return;

        if (previewTile != null && (lHit.transform == previewTile || lHit.transform.IsChildOf(previewTile)))
            return;

        Tile lTile = lHit.transform.GetComponentInParent<Tile>();

        if (lTile == null)
            return;

        if (lTile.tileVariant == Tile.TileVariants.Teleporter
            || lTile.tileVariant == Tile.TileVariants.Target
            || lTile.tileVariant == Tile.TileVariants.Spawner)
            return;

        Tile.TileOrientations lOrientation = GetOrientationFromDirection(lTile.transform.forward);

        UI_Btn_InventoryTile lInventoryTile = UI_Btn_InventoryTile.FindMatchingTile(lTile.tileVariant, lOrientation);

        bool lWasPlacedFromInventory = _PlacedTiles.TryGetValue(lTile.transform, out Transform lGround);

        if (lInventoryTile != null)
        {
            lInventoryTile.AddTileBack();
            lInventoryTile.BeginHandlingFromPickup(lWasPlacedFromInventory);
        }

        if (lWasPlacedFromInventory)
        {
            EnableChild(lGround);
        }

        _PlacedTiles.Remove(lTile.transform);

        Destroy(lTile.gameObject);
    }


    private static Tile.TileOrientations GetOrientationFromDirection(Vector3 pDirection)
    {
        Vector3 lDir = pDirection.normalized;

        float lRightDot = Vector3.Dot(lDir, Vector3.right);
        float lLeftDot = Vector3.Dot(lDir, Vector3.left);
        float lUpDot = Vector3.Dot(lDir, Vector3.forward);
        float lDownDot = Vector3.Dot(lDir, Vector3.back);

        float lMaxDot = lUpDot;
        Tile.TileOrientations lBestOrientation = Tile.TileOrientations.Up;

        if (lRightDot > lMaxDot)
        {
            lMaxDot = lRightDot;
            lBestOrientation = Tile.TileOrientations.Right;
        }

        if (lLeftDot > lMaxDot)
        {
            lMaxDot = lLeftDot;
            lBestOrientation = Tile.TileOrientations.Left;
        }

        if (lDownDot > lMaxDot)
        {
            lBestOrientation = Tile.TileOrientations.Down;
        }

        return lBestOrientation;
    }

    #endregion

    #region _____________________________| ANIMATION

    private static void PlayPlacementTween(Transform pTile, Vector3 pLandingPosition)
    {
        if (pTile == null) return;

        pTile.position = pLandingPosition + Vector3.up * 0.1f;

        Sequence lSequence = DOTween.Sequence();

        lSequence.Join(pTile
            .DOMoveY(pLandingPosition.y, 0.2f)
            .SetEase(Ease.OutBounce)
            .SetLoops(3, LoopType.Restart));

        lSequence.Join(pTile
            .DORotate(pTile.eulerAngles + new Vector3(0f, 360f, 0f), 0.45f, RotateMode.FastBeyond360)
            .SetEase(Ease.InQuad));
    }

    #endregion

        #region _____________________________| EFFECTS

    private void SpawnPlacementDust(Vector3 pPosition, Vector3 pNormal)
    {
        if (_PlacementDustEffect == null) return;

        Vector3 lTangent = Vector3.Cross(Vector3.up, pNormal);

        if (lTangent.sqrMagnitude < 0.001f)
            lTangent = Vector3.Cross(Vector3.right, pNormal);

        Quaternion lRotation = Quaternion.LookRotation(lTangent, pNormal);
        ParticleSystem lDustInstance = Instantiate(_PlacementDustEffect, pPosition + _DustSpawnOffset, lRotation);

        lDustInstance.Play();

        ParticleSystem.MainModule lMain = lDustInstance.main;
        float lDuration = lMain.duration + lMain.startLifetimeMultiplier;

        Destroy(lDustInstance.gameObject, lDuration);
    }
    #endregion

    #region _____________________________| GROUNDS

    private void SetCurrentHitGround(Transform pGround)
    {
        if (_CurrentDisabledGround == pGround) return;

        RestoreCurrentHitGround();
        DiableChild(pGround);
        _CurrentDisabledGround = pGround;
    }

    private void RestoreCurrentHitGround()
    {
        if (_CurrentDisabledGround == null) return;

        EnableChild(_CurrentDisabledGround);
        _CurrentDisabledGround = null;
    }

    private void DetachCurrentHitGround()
    {
        _CurrentDisabledGround = null;
    }

    private void DiableChild(Transform pGround)
    {
        Transform lGroundChild = GetGroundChild(pGround);
        if (lGroundChild == null) return;
        lGroundChild.gameObject.SetActive(false);
    }

    private void EnableChild(Transform pGround)
    {
        Transform lGroundChild = GetGroundChild(pGround);
        if (lGroundChild == null) return;

        lGroundChild.gameObject.SetActive(true);
    }

    private static Transform GetGroundChild(Transform pGround)
    {
        if (pGround == null || pGround.childCount == 0) return null;

        return pGround.GetChild(0);
    }

    private void HandlePlacedTileHover()
    {
        Ray lRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (EventSystem.current.IsPointerOverGameObject())
        {
            RestoreHoveredTile();
            return;
        }

        if (!Physics.Raycast(lRay, out RaycastHit lHit, _RaycastDistance, _TilesLayer))
        {
            RestoreHoveredTile();
            return;
        }

        if (previewTile != null && (lHit.transform == previewTile || lHit.transform.IsChildOf(previewTile)))
        {
            RestoreHoveredTile();
            return;
        }

        Tile lTile = lHit.transform.GetComponentInParent<Tile>();
        if (lTile.tileVariant == Tile.TileVariants.Spawner || lTile.tileVariant == Tile.TileVariants.Target || lTile.tileVariant == Tile.TileVariants.Teleporter)
        {
            RestoreHoveredTile();
            return;
        }
        if (lTile == null)
        {
            RestoreHoveredTile();
            return;
        }

        RaiseHoveredTile(lTile.transform);
    }

    private void RaiseHoveredTile(Transform pTile)
    {
        if (pTile == null || _CurrentRaisedTile == pTile) return;

        RestoreHoveredTile();

        _CurrentRaisedTile = pTile;
        _CurrentRaisedTile.position += Vector3.up * _PlacedTileHoverYOffset;
    }

    private void RestoreHoveredTile(Transform pSpecificTile = null)
    {
        if (_CurrentRaisedTile == null) return;

        if (pSpecificTile != null && pSpecificTile != _CurrentRaisedTile) return;

        _CurrentRaisedTile.position -= Vector3.up * _PlacedTileHoverYOffset;
        _CurrentRaisedTile = null;
    }
    #endregion
}