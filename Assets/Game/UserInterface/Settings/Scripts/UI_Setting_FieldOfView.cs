using Rush.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Rush.UI
{
    public class UI_Setting_FieldOfView : MonoBehaviour
    {
        #region _____________________________/ VALUES

        [SerializeField] private Slider _Slider;
        [SerializeField] private OrbitCamera _TargetCamera;
        [SerializeField] private Vector2 _FoVRange = new(40f, 90f);

        #endregion

        #region _____________________________| UNITY

        private void Awake()
        {
            _Slider ??= GetComponentInParent<Slider>();
            _TargetCamera ??= FindFirstObjectByType<OrbitCamera>();
        }

        private void OnEnable()
        {
            if (_Slider != null && _TargetCamera != null)
                _Slider.value = Mathf.InverseLerp(_FoVRange.x, _FoVRange.y, _TargetCamera.FieldOfView);
        }

        private void Start()
        {
            if (_Slider == null || _TargetCamera == null)
                return;

            _Slider.onValueChanged.AddListener(Apply);
            Apply(_Slider.value);
        }

        #endregion

        #region _____________________________| METHODS

        private void Apply(float pNormalizedValue)
        {
            if (_TargetCamera == null)
                return;

            float lFoV = Mathf.Lerp(_FoVRange.x, _FoVRange.y, Mathf.Clamp01(pNormalizedValue));
            _TargetCamera.FieldOfView = lFoV;
        }

        #endregion
    }
}