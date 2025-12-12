#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Independant
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using UnityEngine;
using UnityEngine.EventSystems;

namespace Rush.Game
{
    [RequireComponent(typeof(Camera))]
    public class MainCamera : MonoBehaviour
    {
        #region ___________________________/ TARGET
        [Header("Target")]
        [SerializeField] private Vector3 _TargetPosition = Vector3.zero;
        #endregion

        #region ___________________________/ SPHERICAL COORDINATES
        [Header("Spherical Coordinates")]
        [SerializeField] private float _Radius = 20;
        [SerializeField] private float _MinRadius = 5f;
        [SerializeField] private float _MaxRadius = 50;
        [SerializeField, Range(0f, 180f)] private float _Colatitude = 60f;
        #endregion

        #region ___________________________/ INPUT SETTINGS
        [Header("Input")]
        [SerializeField] private float _RotationSensitivity = 0.1f;   // Degrees per pixel
        [SerializeField] private float _ScrollZoomSensitivity = 0.2f; // Units per scroll step
        [SerializeField] private float _PinchZoomSensitivity = 0.02f; // Units per pixel
        [SerializeField] private float _InertiaDamping = 4f;
        #endregion

        #region ___________________________/ STATE
        private float      _Theta;
        private float      _RotationVelocity;
        private float      _ZoomVelocity;
        private bool       _IsDragging;
        private bool       _IsPinching;
        private bool       _HasZoomInput;
        private Vector2    _LastPointerPosition;
        private float      _PreviousPinchDistance;
        #endregion

        void Start()
        {
            UpdateCameraPosition();
        }

        void Update()
        {
            HandleInput();
            ApplyInertia(Time.deltaTime);
            UpdateCameraPosition();
        }

        #region ___________________________| INPUTS

        void HandleInput()
        {
            _HasZoomInput = false;

            if (Input.touchCount > 0)
            {
                HandleTouchInput();
            }
            else
            {
                _IsPinching = false;
                HandleMouseInput();
            }

            float lScrollDelta = Input.mouseScrollDelta.y;
            if (!Mathf.Approximately(lScrollDelta, 0f))
            {
                RegisterZoomDelta(-lScrollDelta * _ScrollZoomSensitivity);
            }
        }

        void HandleMouseInput()
        {
            if (IsPointerOverUI())
            {
                _IsDragging = false;
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                _IsDragging = true;
                _LastPointerPosition = Input.mousePosition;
                _RotationVelocity = 0f;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _IsDragging = false;
            }

            if (_IsDragging && Input.GetMouseButton(0))
            {
                Vector2 lCurrentPosition = Input.mousePosition;
                Vector2 lDelta = lCurrentPosition - _LastPointerPosition;
                _LastPointerPosition = lCurrentPosition;

                float lDeltaTheta = lDelta.x * _RotationSensitivity * Mathf.Deg2Rad;
                _Theta += lDeltaTheta;

                if (Time.deltaTime > Mathf.Epsilon)
                    _RotationVelocity = lDeltaTheta / Time.deltaTime;
            }
        }

        void HandleTouchInput()
        {
            if (Input.touchCount == 1)
            {
                Touch lTouch = Input.GetTouch(0);

                if (lTouch.phase == TouchPhase.Began)
                {
                    _IsDragging = true;
                    _LastPointerPosition = lTouch.position;
                    _RotationVelocity = 0f;
                }
                else if (lTouch.phase == TouchPhase.Moved && _IsDragging)
                {
                    Vector2 lDelta = lTouch.deltaPosition;
                    float lDeltaTheta = lDelta.x * _RotationSensitivity * Mathf.Deg2Rad;
                    _Theta += lDeltaTheta;

                    if (Time.deltaTime > Mathf.Epsilon)
                        _RotationVelocity = lDeltaTheta / Time.deltaTime;
                }
                else if (lTouch.phase == TouchPhase.Ended || lTouch.phase == TouchPhase.Canceled)
                {
                    _IsDragging = false;
                }

                _IsPinching = false;
            }
            else if (Input.touchCount >= 2)
            {
                Touch lTouch0 = Input.GetTouch(0);
                Touch lTouch1 = Input.GetTouch(1);

                float lCurrentDistance = Vector2.Distance(lTouch0.position, lTouch1.position);

                if (!_IsPinching)
                {
                    _IsPinching = true;
                    _PreviousPinchDistance = lCurrentDistance;
                }
                else
                {
                    float lDeltaDistance = lCurrentDistance - _PreviousPinchDistance;
                    RegisterZoomDelta(-lDeltaDistance * _PinchZoomSensitivity);
                    _PreviousPinchDistance = lCurrentDistance;
                }

                _IsDragging = false;
            }
        }

        #endregion

        void ApplyInertia(float pDeltaTime)
        {
            if (Mathf.Approximately(pDeltaTime, 0f))
                return;

            if (!_IsDragging && Mathf.Abs(_RotationVelocity) > 0.0001f)
            {
                _Theta += _RotationVelocity * pDeltaTime;
                float lDampingFactor = Mathf.Exp(-_InertiaDamping * pDeltaTime);
                _RotationVelocity *= lDampingFactor;

                if (Mathf.Abs(_RotationVelocity) < 0.0001f)
                    _RotationVelocity = 0f;
            }

            if (!_IsPinching && !_HasZoomInput && Mathf.Abs(_ZoomVelocity) > 0.0001f)
            {
                AdjustRadius(_ZoomVelocity * pDeltaTime);

                float lDampingFactor = Mathf.Exp(-_InertiaDamping * pDeltaTime);
                _ZoomVelocity *= lDampingFactor;

                if (Mathf.Abs(_ZoomVelocity) < 0.0001f)
                    _ZoomVelocity = 0f;
            }
        }

        void AdjustRadius(float pDelta)
        {
            _Radius = Mathf.Clamp(_Radius + pDelta, _MinRadius, _MaxRadius);
        }

        void RegisterZoomDelta(float pDeltaRadius)
        {
            AdjustRadius(pDeltaRadius);

            if (Time.deltaTime > Mathf.Epsilon)
                _ZoomVelocity = pDeltaRadius / Time.deltaTime;
            else
                _ZoomVelocity = 0f;

            _HasZoomInput = true;
        }

        bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }


        void UpdateCameraPosition()
        {
            float lPhi = Mathf.Deg2Rad * _Colatitude;
            float lSinPhi = Mathf.Sin(lPhi);

            Vector3 lOffset = new Vector3
            (
                _Radius * lSinPhi * Mathf.Cos(_Theta),
                _Radius * Mathf.Cos(lPhi),
                _Radius * lSinPhi * Mathf.Sin(_Theta)
            );

            transform.position = _TargetPosition + lOffset;
            transform.LookAt(_TargetPosition, Vector3.up);
        }
    }
}