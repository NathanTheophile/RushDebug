using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rush.UI
{
    [RequireComponent(typeof(Slider))]
    public class UI_Setting_SliderValueLabel : MonoBehaviour
    {
        [SerializeField] private Slider _Slider;
        [SerializeField] private TMP_Text _ValueLabel;
        [SerializeField] private string _Format = "0.00";

        private void Awake()
        {
            _Slider ??= GetComponent<Slider>();
        }

        private void OnEnable()
        {
            if (_Slider == null || _ValueLabel == null)
                return;

            _Slider.onValueChanged.AddListener(UpdateLabel);
            UpdateLabel(_Slider.value);
        }

        private void OnDisable()
        {
            if (_Slider == null)
                return;

            _Slider.onValueChanged.RemoveListener(UpdateLabel);
        }

        private void UpdateLabel(float pValue)
        {
            if (_ValueLabel == null)
                return;

            _ValueLabel.text = pValue.ToString(_Format);
        }
    }
}