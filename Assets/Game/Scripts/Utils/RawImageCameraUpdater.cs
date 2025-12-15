#region _____________________________/ INFOS
//  AUTHOR : OpenAI Assistant (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using UnityEngine;
using UnityEngine.UI;

namespace Rush.UI
{
    /// <summary>
    /// Ensures a RawImage always displays the latest render from a designated camera.
    /// Useful for UI previews driven by a camera living inside a prefab or scene.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class RawImageCameraUpdater : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private Camera _SourceCamera;
        [SerializeField] private Vector2Int _Resolution = new(512, 512);

        private RawImage _RawImage;
        private RenderTexture _RenderTexture;

        private void Awake() => _RawImage = GetComponent<RawImage>();

        private void OnEnable()
        {
            SetupRenderTexture();
            UpdateOutput();
        }

        private void OnDisable()
        {
            CleanupRenderTexture();
        }

        private void Update()
        {
            // Keep the raw image bound to the camera output and render every frame
            // to account for cameras that are instantiated or moved at runtime.
            UpdateOutput();
        }

        private void SetupRenderTexture()
        {
            if (_RenderTexture != null && _RenderTexture.width == _Resolution.x && _RenderTexture.height == _Resolution.y)
                return;

            CleanupRenderTexture();

            _RenderTexture = new RenderTexture(_Resolution.x, _Resolution.y, 24, RenderTextureFormat.ARGB32)
            {
                name = $"RT_{gameObject.name}_CameraPreview"
            };
            _RenderTexture.Create();
        }

        private void UpdateOutput()
        {
            if (_SourceCamera == null)
                return;

            if (_RenderTexture == null)
                SetupRenderTexture();

            if (_SourceCamera.targetTexture != _RenderTexture)
            {
                _SourceCamera.targetTexture = _RenderTexture;
                _SourceCamera.forceIntoRenderTexture = true;
            }

            if (_RawImage.texture != _RenderTexture)
                _RawImage.texture = _RenderTexture;

            if (_SourceCamera.enabled)
                _SourceCamera.Render();
        }

        private void CleanupRenderTexture()
        {
            if (_RenderTexture == null)
                return;

            if (_SourceCamera != null && _SourceCamera.targetTexture == _RenderTexture)
                _SourceCamera.targetTexture = null;

            if (_RenderTexture.IsCreated())
                _RenderTexture.Release();

            Destroy(_RenderTexture);
            _RenderTexture = null;
        }
    }
}