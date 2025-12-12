#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Rush.Game;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Button))]
public class UI_Btn_InventoryTile : MonoBehaviour
{
    #region _____________________________/ REFS

    [Header("References")]
    [SerializeField] private TMP_Text _TileName;
    [SerializeField] private TMP_Text _TileAmount;
    [SerializeField] private RawImage _TileImage;

    [Header("Preview")]
    [SerializeField] private Transform _PreviewCameraPrefab;
    [SerializeField] private Vector3 _PreviewCameraOffset = new(0f, 1f, -2.5f);
    [SerializeField] private int _PreviewTextureSize = 256;
    private bool _ShouldChainPlacement = true;

    #endregion

    #region _____________________________/ VALUES

    public int TileAmount { get; private set; }

    private Button _Button;
    private SO_LevelData.InventoryTile _InventoryTile;

    private static UI_Btn_InventoryTile _CurrentSelectedTile;
    private static readonly List<UI_Btn_InventoryTile> _InventoryTiles = new();
    private bool _IsSelected;

    private Transform _PreviewTileInstance;
    private Transform _PreviewCameraInstance;
    private Camera _PreviewCamera;
    private RenderTexture _PreviewTexture;

    private TilePlacer TilePlacerInstance => TilePlacer.Instance;

    private static readonly Vector3 PreviewSpawnPosition = new(10000f, 10000f, 10000f);
    private static Vector3 _NextPreviewSpawnPosition = PreviewSpawnPosition;

    #endregion

    #region _____________________________| PUBLIC

    public void Initialize(SO_LevelData.InventoryTile pInventoryTile)
    {
        if (_Button == null) _Button = GetComponent<Button>();
        EnsurePreviewImageExists();

        _InventoryTile = pInventoryTile;

        if (!_InventoryTiles.Contains(this))
            _InventoryTiles.Add(this);

        TileAmount = pInventoryTile.quantity;

        _TileName.text = pInventoryTile.type.ToString();
        _TileAmount.text = TileAmount.ToString();

        SetupTilePreview();


        _Button.onClick.RemoveAllListeners();
        _Button.onClick.AddListener(() =>
        {
            if (TilePlacerInstance == null)
            {
                Debug.LogWarning("TilePlacer reference missing. Cannot set tile prefabs from inventory button.");
                return;
            }

            if (_InventoryTile.tilePrefab == null)
            {
                Debug.LogWarning($"No tile prefab assigned for {_InventoryTile.type} in level data.");
                return;
            }

            if (_CurrentSelectedTile != null && _CurrentSelectedTile != this) _CurrentSelectedTile._IsSelected = false;

            _CurrentSelectedTile = this;
            _IsSelected = true;

            TilePlacerInstance.OnTilePlaced -= HandleTilePlaced;
            TilePlacerInstance.OnTilePlaced += HandleTilePlaced;
            TilePlacerInstance.StartHandlingTile();
            _ShouldChainPlacement = true;

            TilePlacerInstance.SetTilePrefabs(_InventoryTile.tilePrefab, _InventoryTile.previewPrefab, _InventoryTile.orientation);
        });
    }
    public SO_LevelData.InventoryTile GetInventoryData()
    {
        return new SO_LevelData.InventoryTile
        {
            type = _InventoryTile.type,
            orientation = _InventoryTile.orientation,
            quantity = TileAmount,
            tilePrefab = _InventoryTile.tilePrefab,
            previewPrefab = _InventoryTile.previewPrefab
        };
    }
    public bool ConsumeTile()
    {
        TileAmount--;

        if (TileAmount <= 0)
        {
            TileAmount = 0;
            _TileAmount.text = TileAmount.ToString();
            SetButtonInteractable(false);
            return false;
        }

        _TileAmount.text = TileAmount.ToString();
        return true;
    }
    public static List<SO_LevelData.InventoryTile> GetInventorySnapshot()
    {
        List<SO_LevelData.InventoryTile> lSnapshot = new();

        foreach (UI_Btn_InventoryTile lTile in _InventoryTiles)
        {
            if (lTile == null) continue;

            lSnapshot.Add(lTile.GetInventoryData());
        }

        return lSnapshot;
    }

    public void AddTileBack()
    {
        TileAmount++;
        _TileAmount.text = TileAmount.ToString();

        if (!_Button.interactable)
            SetButtonInteractable(true);
    }

    #endregion

    #region _____________________________| UNITY

    private void OnDestroy()
    {
        _InventoryTiles.Remove(this);

        if (TilePlacerInstance != null) TilePlacerInstance.OnTilePlaced -= HandleTilePlaced;
                CleanupPreview();
    }

        private void Update() => UpdatePreviewCameraTransform();


    #endregion

    #region _____________________________| PRIVATE

    private void HandleTilePlaced()
    {
        if (!_IsSelected) return;

        if (!ConsumeTile())
        {
            _IsSelected = false;

            if (_CurrentSelectedTile == this) _CurrentSelectedTile = null;

            if (TilePlacerInstance != null) TilePlacerInstance.ClearSelection();
        }
        else if (TilePlacerInstance != null && _ShouldChainPlacement)
        {
            TilePlacerInstance.StartHandlingTile();
            TilePlacerInstance.SetTilePrefabs(_InventoryTile.tilePrefab, _InventoryTile.previewPrefab, _InventoryTile.orientation);
        }
        else
        {
            _IsSelected = false;

            if (_CurrentSelectedTile == this) _CurrentSelectedTile = null;

            if (TilePlacerInstance != null) TilePlacerInstance.ClearSelection();
        }
    }

    private void SetButtonInteractable(bool pIsInteractable)
    {
        if (_Button != null)
            _Button.interactable = pIsInteractable;

        if (_TileImage != null)
            _TileImage.color = pIsInteractable ? Color.white : new Color(1f, 1f, 1f, 0.5f);
    }

    private void SetupTilePreview()
    {
        CleanupPreview();

        if (_TileImage == null)
            return;

        if (_InventoryTile.tilePrefab == null)
        {
            Debug.LogWarning($"No tile prefab assigned for {_InventoryTile.type} in level data.");
            return;
        }

        if (_PreviewCameraPrefab == null)
        {
            Debug.LogWarning("Preview camera prefab missing for inventory tile preview.");
            return;
        }

        _PreviewTileInstance = Instantiate(_InventoryTile.tilePrefab, _NextPreviewSpawnPosition, GetRotationFromOrientation(_InventoryTile.orientation));
        _NextPreviewSpawnPosition += new Vector3(1000f, 0f, 0f);
        _PreviewTileInstance.gameObject.name = $"{_InventoryTile.type}_Preview";

        _PreviewCameraInstance = Instantiate(_PreviewCameraPrefab, _PreviewTileInstance);
        _PreviewCameraInstance.localPosition = Vector3.zero;
        _PreviewCameraInstance.localRotation = Quaternion.identity;

        _PreviewCamera = _PreviewCameraInstance.GetComponentInChildren<Camera>();

        if (_PreviewCamera == null)
        {
            Debug.LogWarning("Preview camera component not found on preview prefab.");
            return;
        }

        if (_PreviewCamera.TryGetComponent(out AudioListener lAudioListener))
            Destroy(lAudioListener);

        if (_PreviewCamera.TryGetComponent(out OrbitCamera lMainCameraComponent))
            Destroy(lMainCameraComponent);

        if (_PreviewCamera.TryGetComponent(out UniversalAdditionalCameraData lCameraData))
            lCameraData.renderType = CameraRenderType.Base;

        _PreviewCamera.clearFlags = CameraClearFlags.SolidColor;
        _PreviewCamera.backgroundColor = Color.clear;

        _PreviewTexture = new RenderTexture(_PreviewTextureSize, _PreviewTextureSize, 16)
        {
            antiAliasing = 2,
            name = $"RT_{_InventoryTile.type}_Preview"
        };

        _PreviewCamera.targetTexture = _PreviewTexture;
        _TileImage.texture = _PreviewTexture;
        _TileImage.enabled = true;

        UpdatePreviewCameraTransform();
    }

    private void UpdatePreviewCameraTransform()
    {
        if (_PreviewCamera == null || _PreviewTileInstance == null)
            return;

        Camera lMainCamera = Camera.main;

        if (lMainCamera == null)
            return;

        Vector3 lOffset = lMainCamera.transform.TransformVector(_PreviewCameraOffset);
        _PreviewCamera.transform.position = _PreviewTileInstance.position + lOffset / 4;

        _PreviewCamera.transform.LookAt(_PreviewTileInstance.position, lMainCamera.transform.up);
    }

    private void CleanupPreview()
    {
        if (_PreviewCamera != null)
            _PreviewCamera.targetTexture = null;

        if (_PreviewTexture != null)
        {
            _PreviewTexture.Release();
            Destroy(_PreviewTexture);
        }

        if (_PreviewCameraInstance != null)
            Destroy(_PreviewCameraInstance.gameObject);

        if (_PreviewTileInstance != null)
            Destroy(_PreviewTileInstance.gameObject);

        _PreviewTileInstance = null;
        _PreviewCameraInstance = null;
        _PreviewCamera = null;
        _PreviewTexture = null;
    }

    private void EnsurePreviewImageExists()
    {
        if (_TileImage != null)
            return;

        _TileImage = GetComponentInChildren<RawImage>(true);

        if (_TileImage != null)
            return;

        GameObject lPreviewObject = new("Img_Preview", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        RectTransform lRect = lPreviewObject.GetComponent<RectTransform>();
        lRect.SetParent(transform, false);
        lRect.anchorMin = Vector2.zero;
        lRect.anchorMax = Vector2.one;
        lRect.offsetMin = Vector2.zero;
        lRect.offsetMax = Vector2.zero;
        lRect.SetAsFirstSibling();

        _TileImage = lPreviewObject.GetComponent<RawImage>();
        _TileImage.raycastTarget = false;
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
    private bool MatchesTile(Tile.TileVariants pType, Tile.TileOrientations pOrientation)
        => _InventoryTile.type == pType && _InventoryTile.orientation == pOrientation;

    public void BeginHandlingFromPickup(bool pWasPlacedFromInventory)
    {
        if (TilePlacerInstance == null)
        {
            Debug.LogWarning("TilePlacer reference missing. Cannot begin handling from pickup.");
            return;
        }

        if (_CurrentSelectedTile != null && _CurrentSelectedTile != this)
            _CurrentSelectedTile._IsSelected = false;

        _CurrentSelectedTile = this;
        _IsSelected = true;
        _ShouldChainPlacement = pWasPlacedFromInventory;

        TilePlacerInstance.OnTilePlaced -= HandleTilePlaced;
        TilePlacerInstance.OnTilePlaced += HandleTilePlaced;

        TilePlacerInstance.StartHandlingTile();
        TilePlacerInstance.SetTilePrefabs(_InventoryTile.tilePrefab, _InventoryTile.previewPrefab, _InventoryTile.orientation);
    }


    #endregion

    #region _____________________________| STATIC

    public static UI_Btn_InventoryTile FindMatchingTile(Tile.TileVariants pType, Tile.TileOrientations pOrientation)
    {
        return _InventoryTiles.Find(lTile => lTile != null && lTile.MatchesTile(pType, pOrientation));
    }

    public static void ResetSelection()
    {
        if (_CurrentSelectedTile != null)
        {
            _CurrentSelectedTile._IsSelected = false;
            _CurrentSelectedTile = null;
        }

        TilePlacer.Instance?.ClearSelection();
    }

    #endregion
}