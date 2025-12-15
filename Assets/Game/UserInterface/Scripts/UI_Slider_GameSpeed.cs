using System.Security.Cryptography;
using Rush.Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rush.UI
{
    public class UI_Slider_GameSpeed : MonoBehaviour
    {
        #region _____________________________/ VALUES

        [SerializeField] private Slider _GameSpeedSlider;
        [SerializeField] private TMP_Text _GameSpeedValueText;

        #endregion

        #region _____________________________| UNITY

        private void Awake()
        {
            _GameSpeedSlider ??= GetComponentInParent<Slider>();
            _GameSpeedValueText ??= GetComponentInChildren<TMP_Text>();
        }

        private void Start()
        {
            if (_GameSpeedSlider == null)
                return;

            _GameSpeedSlider.onValueChanged.AddListener(OnSliderValueChanged);
            ApplySliderValue(1f, true);
        }

        private void OnEnable()
        {
            ApplySliderValue(1f, true);
        }

        #endregion

        #region _____________________________| CALLBACKS

        private void OnSliderValueChanged(float pValue) => ApplySliderValue(pValue);

        private void ApplySliderValue(float pValue, bool pSetSliderValue = false)
        {
            if (_GameSpeedSlider == null)
                return;

            if (pSetSliderValue)
                _GameSpeedSlider.SetValueWithoutNotify(pValue);

            Manager_Time.Instance.GlobalTickSpeed = pValue;

            if (_GameSpeedValueText != null)
                _GameSpeedValueText.text = pValue.ToString("0.0");
        }

        #endregion
    }
}