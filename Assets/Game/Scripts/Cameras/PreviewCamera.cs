#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Independant
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using UnityEngine;

namespace Rush.Game
{
    [RequireComponent(typeof(Camera))]
    public class PreviewCamera : MonoBehaviour
    {
        #region ___________________________/ TARGET
        [Header("Target")]
        [SerializeField] private Vector3 _TargetPosition = Vector3.zero;
        #endregion

        #region ___________________________/ SPHERICAL COORDINATES
        [Header("Spherical Coordinates")]
        [SerializeField] private float _Radius = 20f;
        [SerializeField, Range(0f, 180f)] private float _Colatitude = 60f;
        [SerializeField] private float _RotateSpeed = 45f;
        #endregion

        #region ___________________________/ STATE
        private float _Theta;

        public bool canRotate;

        #endregion

        void Start() => UpdateCameraPosition();

        void Update()
        {
            if (canRotate) RotatePreview();

            UpdateCameraPosition();
        }

        public void AddTargetWorldOffset(Vector3 pWorldOffset) => _TargetPosition += pWorldOffset;

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

        void RotatePreview() => _Theta += _RotateSpeed * Mathf.Deg2Rad * Time.deltaTime;
    }
}