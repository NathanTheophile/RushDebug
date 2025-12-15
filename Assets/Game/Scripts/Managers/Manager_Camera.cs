#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Singleton
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using UnityEngine;

namespace Rush.Game
{
    public class Manager_Camera : MonoBehaviour
    {
        #region _____________________________/ SINGLETON

        public static Manager_Camera Instance { get; private set; }

        private void CheckForInstance()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        #endregion

        #region _____________________________/ REFS

        [SerializeField] private Camera _MenuCamera;
        [SerializeField] private Camera _GameCamera;
        public Camera ActiveCamera { get; private set; }

        #endregion

        #region _____________________________| UNITY

        private void Awake()
        {
            CheckForInstance();
            SetMenuCameraActive();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        #endregion

        #region _____________________________| METHODS

        public void SetMenuCameraActive() => SetActiveCamera(_MenuCamera);

        public void SetGameCameraActive() => SetActiveCamera(_GameCamera);

        public Camera GetActiveCameraOrMain()
        {
            if (ActiveCamera != null)
                return ActiveCamera;

            return Camera.main;
        }

        private void SetActiveCamera(Camera pTarget)
        {
            if (pTarget == null)
                return;

            if (_MenuCamera != null)
                _MenuCamera.gameObject.SetActive(pTarget == _MenuCamera);

            if (_GameCamera != null)
                _GameCamera.gameObject.SetActive(pTarget == _GameCamera);

            ActiveCamera = pTarget;
            Camera.SetupCurrent(pTarget);
        }

        #endregion
    }
}