#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using Rush.Game;
using Rush.Game.Core;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rush.UI
{
    public class UI_Btn_Level : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region _____________________________/ ELEMENTS

        [SerializeField] private TMP_Text _PrefabNameText;
        [SerializeField] private RawImage _PreviewImage;
        [SerializeField] private Button _BtnLevel;
        [SerializeField] private Transform _PanelToShow;
        [SerializeField] private Camera _CameraPrefab;
        private RenderTexture _PreviewTexture;
        private Transform _RootCard;
        public GameObject instantiatedLevel { get; private set; }
        [SerializeField] private Vector2Int _PreviewResolution = new Vector2Int(512, 512);

        #endregion

        #region _____________________________/ DATA

        private SO_LevelData _LevelData;

        #endregion

        #region _____________________________/ CAMERA

        private Camera _Camera;
        private PreviewCamera _PreviewCamera;

        #endregion

        #region _____________________________/ DYNAMICS

        private float _HoveringScale = 1.1f;

        #endregion

        #region _____________________________| INIT

        private void Awake() {
            _BtnLevel = GetComponent<Button>();
            _BtnLevel.onClick.AddListener(OnButtonClicked); }



        public void Initialize(Vector3 pSpawnPosition, SO_LevelData pLevelData, Transform pRootCard)
        {
            CleanupTexture();

            _RootCard = pRootCard;

            _LevelData = pLevelData;
            _PrefabNameText.text = _LevelData.levelName;


            instantiatedLevel = Instantiate(_LevelData.levelPrefab, pSpawnPosition, Quaternion.identity, _RootCard);

            _Camera = Instantiate(_CameraPrefab, instantiatedLevel.transform);
            if (_Camera == null)
            {
                Debug.LogError("Preview camera prefab instantiation failed.");
                return;
            }

            if (!_Camera.TryGetComponent(out _PreviewCamera))
            {
                Debug.LogWarning("PreviewCamera component missing on preview camera prefab. Adding one at runtime to avoid breaking the level selector preview.");
                _PreviewCamera = _Camera.gameObject.AddComponent<PreviewCamera>();
            }
            else Debug.LogWarning("PreviewCamera component found.");

            _PreviewCamera.AddTargetWorldOffset(pSpawnPosition);


            _PreviewTexture = new RenderTexture(_PreviewResolution.x, _PreviewResolution.y, 24)
            {
                name = $"RT_{_LevelData.name}_Preview"
            };

            _PreviewTexture.Create();

            _Camera.targetTexture = _PreviewTexture;
                        _Camera.forceIntoRenderTexture = true;

            _Camera.enabled = true;
                        _Camera.Render();

            _PreviewImage.texture = _PreviewTexture;

        }

        private void CleanupTexture()
        {
            ReleasePreviewCamera();

            if (_PreviewTexture != null)
            {
                if (_PreviewTexture.IsCreated()) _PreviewTexture.Release();

                Destroy(_PreviewTexture);
                _PreviewTexture = null;
            }

            _PreviewImage.texture = null;
            _Camera = null;
            _PreviewCamera = null;
        }

        private void ReleasePreviewCamera()
        {
            if (_Camera == null) return;

            _Camera.targetTexture = null;

            if (_PreviewCamera != null)
            {
                _PreviewCamera.canRotate = false;
            }
        }

        private void OnDestroy()
        {
            CleanupTexture();
        }

        #endregion        

        #region _____________________________| MOUSE EVENTS

        public void OnPointerEnter(PointerEventData eventData) {
            _PreviewCamera.canRotate = true;
            transform.localScale = Vector3.one * _HoveringScale; }

        public void OnPointerExit(PointerEventData eventData) {
            _PreviewCamera.canRotate = false;
            transform.localScale = Vector3.one; }

        private void OnButtonClicked() {
            CleanupTexture();
            Manager_Game.Instance.SpawnCurrentLevel(_LevelData);
            Manager_Ui.Instance.Switch(_PanelToShow, _RootCard);
        }

        #endregion
    }
}