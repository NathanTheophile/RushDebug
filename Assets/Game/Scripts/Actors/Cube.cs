#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using System;
using System.Collections.Generic;
using DG.Tweening;
using Rush.Game.Core;
using UnityEngine;
using static Rush.Game.SO_Colors;

namespace Rush.Game
{
    public class Cube : MonoBehaviour, ITickDependant
    {
        #region _________________________/ MAIN VALUES
        [Header("Main")]
        [SerializeField] Material transparent;
        private Transform _Self;
        private float _GridSize = 1f;
        private Action doAction;

        public Material Color { get;  private set; }
        public Tween GetValidationTween() => _ValidationTween;
        public ColorsData ColorData;
        [SerializeField] private GameObject _CollisionUIPrefab;
        [SerializeField] private Vector3 _CollisionUIOffset = Vector3.up;
        [SerializeField] Material _Material;
        [Header("VFX")]
        [SerializeField, Min(0f)] private float _ValidationTweenDuration = 5f;
        [SerializeField, Min(0f)] private float _ValidationJumpDuration = 2f;
        [SerializeField, Min(0f)] private float _ValidationJumpPower = 2f;
        private Tween _ValidationTween;
        #endregion

        #region _________________________/ TIME VALUES
        [Header("Time")]
        public float currentTickStep { get; set; }
        public int levelStopperTicks { get; private set; } = 2;
        private int pauseTicksRemaining = 0;

        #endregion

        #region _________________________/ MOVEMENT VALUES
        [Header("Movement")]
        private Vector3 _Direction = Vector3.forward;

        private float _BaseAngle = 90f;
        private Vector3 _PivotPoint;
        private Quaternion _StartRotation, _EndRotation;
        private Vector3 _StartPosition, _EndPosition;

        public bool justTeleported = false;

        // On utilise des directions logiques pr ne pas avoir à rotate le transform, on stocke les 4 directions dans une liste,
        // on bouclera sur les 4 directions à partir de la direction actuelle en modulant par par 4 poru rester entre 0 et 3
        // et pouvoir check facilement les 3 directions peut importe la direction actuel du cube
        static readonly Vector3Int[] DIRECTIONS = { Vector3Int.forward, Vector3Int.right, Vector3Int.back, Vector3Int.left };
        static int Right(int i) => (i + 1) % DIRECTIONS.Length;
        static int Back(int i) => (i + 2) % DIRECTIONS.Length;
        static int Left(int i) => (i + 3) % DIRECTIONS.Length;
        private int DirectionIndexOf(Vector3Int pDirection) => Array.IndexOf(DIRECTIONS, pDirection);

        #endregion

        #region _________________________/ COLLISION VALUES
        [Header("Collisions")]
        [SerializeField] private LayerMask _GroundLayer;
        [SerializeField] private LayerMask _TilesLayer;
        public event Action<Cube, RaycastHit> onTileDetected;
        public event Action<Cube> onCubeDeath;

        #endregion

        #region _________________________| INIT
        private void Awake()
        {
            _Self = transform;
            doAction = Pause;
        }

        public void SpawnDirection(Vector3 pDirection) => _Direction = pDirection;

        #endregion

        #region _________________________| GAME FLOW METHODS

        private void Update() => doAction();

        public void TickUpdate(int pTickIndex)
        {
            doAction(); // Petite execution pour appliquer la dernière step du tick ajustée à 1 dans le TImeManger

            if (pauseTicksRemaining > 0)
            {
                pauseTicksRemaining--;
                if (pauseTicksRemaining == 0) SetModeRoll();
                return;
            }
            
            SetNextMode();
        }

        private void SetNextMode() => TryFindGround(out _);


        #endregion

        #region _________________________| PHYSIC METHODS

        /// <returns>retourne si le raycast a détecté qq chose et si tile retourne tile sinon null</returns>
        private void TryFindGround(out RaycastHit pHit)
        {
            float lMaxDistance = _GridSize * 3f;

            if (Physics.Raycast(_Self.position, Vector3.down, out pHit, lMaxDistance, _GroundLayer | _TilesLayer))
            {
                if (pHit.distance > _GridSize)
                {
                    SetModeSlide(Vector3.down);
                    return;
                }

                if (pHit.transform.gameObject.layer == 7)
                    onTileDetected?.Invoke(this, pHit);

                else SetModeRoll(_Direction);
            }
            else 
            {
                SetModePause();
                TriggerCubeDeath();
            }
        }

        private bool LookAround()
        {
            var lCheckingOrder = SetSidesCheckingOrder(); // là je sors une liste de direction à check en fonction de la direction actuelle
            return FindNewDirection(lCheckingOrder);
        } // Je set une nouvelle direction et dedans je gère la pause

        /// <summary>
        /// on récupère l'index des directions correpsondant à la driection actuelle du cube, puis on définit depuis
        /// cette index les directiction du transform correspondant aux trois directions à checker dans l'ordre d'apres la direction actuelle
        /// </summary>
        /// <param name="currentOrientation">l'orientation LOGIQUE actuelle du cube</param>
        /// <returns></returns>
        private Vector3Int[] SetSidesCheckingOrder()
        {
            int i = DirectionIndexOf(Vector3Int.RoundToInt(_Direction));
            return new[] { DIRECTIONS[i], DIRECTIONS[Right(i)], DIRECTIONS[Back(i)], DIRECTIONS[Left(i)] };
        }

        private bool CheckForWall(Vector3Int pDirection) => Physics.Raycast(_Self.position, pDirection, _GridSize, _GroundLayer); //oeoe le raycast

        /// <summary>
        /// on se base sur la liste pour check les 4 directions depuis la direction actuelle et on sort si un checkwall renvoie false
        /// </summary>
        /// <param name="pCheckingOrder"></param>
        private bool FindNewDirection(IEnumerable<Vector3Int> pCheckingOrder)
        {
            foreach (var lDirection in pCheckingOrder)
            {
                if (CheckForWall(lDirection))
                {
                    PlaySquashStretch(lDirection);

                    continue;

                }
                if (lDirection != _Direction) //là il a trouvé une direction ou il prend rien dans la goule
                {
                    _Direction = lDirection;
                    return true;
                }

                break;
            }
            return false;
        }

        void OnCollisionEnter(Collision other) => HandleCubeCollision(other.gameObject);

        void OnTriggerEnter(Collider other) => HandleCubeCollision(other.gameObject);

        private void HandleCubeCollision(GameObject other)
        {
            if (other.TryGetComponent(out Cube _))
                TriggerCubeDeath();
        }
        
        #endregion

        #region _________________________| STATE MACHINE SETTERS

        public void SetModePause(int pTicks = 1) {
            pauseTicksRemaining = Mathf.Max(pauseTicksRemaining, pTicks);
            doAction = Pause; }

        public void SetModeRoll(Vector3 pDirection = default)
        {
            if (pDirection == default) pDirection = _Direction;
            else _Direction = Vector3Int.RoundToInt(pDirection);

            if (LookAround())
            {
                SetModePause();
                return;
            }

            Vector3 lAxis = Vector3.Cross(Vector3.up, _Direction);
            _PivotPoint = _Self.position + (Vector3.down + _Direction) * (_GridSize / 2f);

            _StartRotation = _Self.rotation;
            _EndRotation = Quaternion.AngleAxis(_BaseAngle, lAxis) * _StartRotation;

            GetLerpMovement(_Self.position - _PivotPoint, _Direction);

            doAction = Roll;
        }

        public void SetModeSlide(Vector3 pSlideDirection)
        {
            GetLerpMovement(_Self.position, pSlideDirection);
            doAction = Slide;
        }

        public void SetModeTeleportation(Vector3 pTarget)
        {
            _EndPosition = pTarget;
            doAction = Teleport;
        }

        #endregion

        #region _________________________| STATES

        private void Pause() { }

        private void Roll()
        {
            _Self.rotation = Quaternion.Slerp(_StartRotation, _EndRotation, currentTickStep);
            _Self.position = _PivotPoint + Vector3.Slerp(_StartPosition, _EndPosition, currentTickStep);
        }


        private void Slide() => _Self.position = Vector3.Lerp(_StartPosition, _EndPosition, currentTickStep);

        private void Teleport()
        {
            float lSizeRatio = Mathf.Sin(currentTickStep * Mathf.PI);
            float lSize = 1f - lSizeRatio;
            _Self.localScale = Vector3.one * lSize;
            if (currentTickStep > .5f) _Self.position = _EndPosition;
        }

        #endregion

        #region _________________________| MISC METHODS
        private void TriggerCubeDeath()
        {
            SpawnCollisionUI();
            Manager_Time.Instance.SetPauseStatus(true);
            onCubeDeath?.Invoke(this);
        }

        private void SpawnCollisionUI()
        {
            if (_CollisionUIPrefab == null) return;

            Vector3 spawnPosition = _Self.position + _CollisionUIOffset;
            Instantiate(_CollisionUIPrefab, spawnPosition, Quaternion.identity);
        }
        private void GetLerpMovement(Vector3 pOrigin, Vector3 pDirection)
        {
            _StartPosition = pOrigin;
            _EndPosition = _StartPosition + pDirection * _GridSize;
        }

        public void SetColor(Material pCubeMaterial)
        {
            Color = pCubeMaterial;
            GetComponentInParent<Renderer>().material = Color;
        }
        private void PlaySquashStretch(Vector3 impactDirection)
        {
            // Retrieve latest global tick speed
            float globalSpeed = Manager_Time.Instance.GlobalTickSpeed;
            if (globalSpeed <= 0f) globalSpeed = 1f;

            // Base duration from currentTickStep, then scaled by global tick speed
            float duration = currentTickStep / globalSpeed;

            // Kill any existing squash/stretch on this transform
            _Self.DOKill();

            Vector3 originalScale = Vector3.one;

            float squashAmount = 0.7f;
            float stretchAmount = 1.2f;

            Vector3 absDirection = new Vector3(Mathf.Abs(impactDirection.x), Mathf.Abs(impactDirection.y), Mathf.Abs(impactDirection.z));
            Vector3 squashScale = new Vector3(stretchAmount, stretchAmount, stretchAmount);

            if (absDirection.x > 0f)
                squashScale.x = squashAmount;
            if (absDirection.y > 0f)
                squashScale.y = squashAmount;
            if (absDirection.z > 0f)
                squashScale.z = squashAmount;

            Sequence seq = DOTween.Sequence();
            seq.Append(_Self.DOScale(squashScale, duration * 0.5f));
            seq.Append(_Self.DOScale(originalScale, duration * 0.5f));
        }


        public void PlayValidationTween(Action onComplete)
        {
            Debug.Log("Joue anim");
                        GetComponent<Collider>().enabled = false;

            Renderer renderer = GetComponent<Renderer>();
            _ValidationTween?.Kill();

            Sequence validationSequence = DOTween.Sequence();
            validationSequence.OnKill(() => _ValidationTween = null);
            Vector3 targetPosition = _Self.position + Vector3.up * _ValidationJumpPower;

            if (renderer == null)
            {
                validationSequence.Append(_Self.DOMoveY(targetPosition.y, _ValidationJumpDuration));
                validationSequence.Join(_Self.DORotate(Vector3.zero, _ValidationJumpDuration, RotateMode.Fast));
                validationSequence.OnComplete(() => onComplete?.Invoke());
                _ValidationTween = validationSequence;
                return;
            }

            Material material = renderer.material;
                        var mats = new List<Material>() { material, transparent };
            GetComponent<Renderer>().SetMaterials(mats);
            float height = 1f;
            material.SetFloat("_CutoffHeight", height);

            validationSequence.Append(_Self.DOMoveY(targetPosition.y, _ValidationJumpDuration));
            validationSequence.Join(_Self.DORotate(Vector3.zero, _ValidationJumpDuration, RotateMode.Fast));
            validationSequence.Join(DOTween.To(() => height, value =>
            {
                height = value;
                material.SetFloat("_CutoffHeight", height);
            }, -1f, _ValidationTweenDuration));

            validationSequence.OnComplete(() => onComplete?.Invoke());
            _ValidationTween = validationSequence;
        }

        private void OnDestroy() => _ValidationTween?.Kill();

        #endregion
    }

}