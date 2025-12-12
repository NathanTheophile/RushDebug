using Rush.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Rush.UI
{
    public class UI_Setting_MouseSensitivity : MonoBehaviour
    {
        #region _____________________________/ VALUES

        [SerializeField] private Slider _Slider;
        [SerializeField] private OrbitCamera _TargetCamera;
        [SerializeField] private float _ScrollMultiplier = 0.2f;

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
                _Slider.value = _TargetCamera.ScrollSensitivity / _ScrollMultiplier;
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

        private void Apply(float pValue)
        {
            if (_TargetCamera == null)
                return;

            _TargetCamera.ScrollSensitivity = Mathf.Max(0f, pValue * _ScrollMultiplier);
        }

        #endregion
    }
}