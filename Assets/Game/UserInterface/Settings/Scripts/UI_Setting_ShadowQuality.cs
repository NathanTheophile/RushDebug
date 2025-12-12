using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Rush.UI
{
    public class UI_Setting_ShadowQuality : MonoBehaviour
    {
        #region _____________________________/ VALUES

        [SerializeField] private TMP_Dropdown _Dropdown;

        #endregion

        #region _____________________________| UNITY

        private void Awake() => _Dropdown ??= GetComponentInChildren<TMP_Dropdown>();

        private void OnEnable()
        {
            if (_Dropdown != null)
                _Dropdown.value = (int)QualitySettings.shadows;
        }

        private void Start()
        {
            if (_Dropdown == null)
                return;

            _Dropdown.onValueChanged.AddListener(Apply);
            Apply(_Dropdown.value);
        }

        #endregion

        #region _____________________________| METHODS

        private void Apply(int pIndex)
        {
            pIndex = Mathf.Clamp(pIndex, (int)ShadowQuality.Disable, (int)ShadowQuality.All);
            QualitySettings.shadows = (ShadowQuality)pIndex;
        }

        #endregion
    }
}