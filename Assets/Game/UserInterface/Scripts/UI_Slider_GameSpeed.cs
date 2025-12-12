using System.Security.Cryptography;
using Rush.Game.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Rush.UI
{
    public class UI_Slider_GameSpeed : MonoBehaviour
    {
        #region _____________________________/ VALUES

        [SerializeField] private Slider _GameSpeedSlider;

        #endregion

        #region _____________________________| UNITY

        private void Awake() => _GameSpeedSlider ??= GetComponentInParent<Slider>();

        private void Start()
        {
            if (_GameSpeedSlider == null)
                return;

            _GameSpeedSlider.onValueChanged.AddListener(OnSliderValueChanged);
            _GameSpeedSlider.value = 1f;
            Manager_Time.Instance.GlobalTickSpeed = 1f;
        }

        private void OnEnable()
        {
                        _GameSpeedSlider.value = 1f;
            Manager_Time.Instance.GlobalTickSpeed = 1f;
        }

        #endregion

        #region _____________________________| CALLBACKS

        private void OnSliderValueChanged(float pValue) => Manager_Time.Instance.GlobalTickSpeed = pValue;

        #endregion
    }
}