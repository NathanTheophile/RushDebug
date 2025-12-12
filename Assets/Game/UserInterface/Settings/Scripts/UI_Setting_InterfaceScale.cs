using UnityEngine;
using UnityEngine.UI;

namespace Rush.UI
{
    public class UI_Setting_InterfaceScale : MonoBehaviour
    {
        #region _____________________________/ VALUES

        [SerializeField] private Slider _Slider;
        [SerializeField] private CanvasScaler _TargetScaler;
        [SerializeField, Range(0.25f, 3f)] private float _MinScale = 0.75f;
        [SerializeField, Range(0.25f, 3f)] private float _MaxScale = 1.25f;

        #endregion

        #region _____________________________| UNITY

        private void Awake()
        {
            if (_Slider == null)
                _Slider = GetComponentInParent<Slider>();

            if (_TargetScaler == null)
                _TargetScaler = FindFirstObjectByType<CanvasScaler>();
        }

        private void OnEnable()
        {
            if (_Slider != null)
                _Slider.value = GetCurrentNormalizedScale();
        }

        private void Start()
        {
            if (_Slider == null)
                return;

            _Slider.onValueChanged.AddListener(ApplyScale);
            ApplyScale(_Slider.value);
        }

        #endregion

        #region _____________________________| METHODS

        private float GetCurrentNormalizedScale()
        {
            if (_TargetScaler == null)
                return 0.5f;

            float lT = Mathf.InverseLerp(_MinScale, _MaxScale, _TargetScaler.scaleFactor);
            return Mathf.Clamp01(lT);
        }

        private void ApplyScale(float pNormalizedValue)
        {
            if (_TargetScaler == null)
                return;

            float lScale = Mathf.Lerp(_MinScale, _MaxScale, Mathf.Clamp01(pNormalizedValue));
            _TargetScaler.scaleFactor = lScale;
        }

        #endregion
    }
}