using Rush.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Rush.UI
{
    public class UI_Setting_CameraSensitivity : MonoBehaviour
    {
        #region _____________________________/ VALUES

        [SerializeField] private Slider _Slider;
        [SerializeField] private OrbitCamera _TargetCamera;
        [SerializeField] private float _Multiplier = 0.5f;

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
                _Slider.value = _TargetCamera.RotationSensitivity / _Multiplier;
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

            _TargetCamera.RotationSensitivity = Mathf.Max(0f, pValue * _Multiplier);
        }

        #endregion
    }
}