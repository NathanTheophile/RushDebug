using Rush.Game.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Rush.UI
{
    public class UI_Setting_AudioVolume : MonoBehaviour
    {
        #region _____________________________/ VALUES

        [SerializeField] private Slider _Slider;
        [SerializeField] private string _MixerGroupName = "Master";

        #endregion

        #region _____________________________| UNITY

        private void Awake() => _Slider ??= GetComponentInParent<Slider>();

        private void OnEnable()
        {
            if (_Slider == null)
                return;

            float lDefault = _Slider.value;
            if (Manager_Audio.Instance != null)
                _Slider.value = Manager_Audio.Instance.GetGroupVolume(_MixerGroupName, lDefault);
        }

        private void Start()
        {
            if (_Slider == null)
                return;

            _Slider.onValueChanged.AddListener(Apply);
            Apply(_Slider.value);
        }

        #endregion

        #region _____________________________| METHODS

        private void Apply(float pValue)
        {
            if (Manager_Audio.Instance == null)
                return;

            Manager_Audio.Instance.SetGroupVolume(_MixerGroupName, pValue);
        }

        #endregion
    }
}