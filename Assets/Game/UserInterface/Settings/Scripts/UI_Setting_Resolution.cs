using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Rush.UI
{
    public class UI_Setting_Resolution : MonoBehaviour
    {
        #region _____________________________/ VALUES

        [SerializeField] private TMP_Dropdown _Dropdown;

        private readonly List<Vector2Int> _SupportedResolutions = new()
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1600, 900),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440),
            new Vector2Int(3840, 2160),
            new Vector2Int(2560, 1080),
            new Vector2Int(3440, 1440),
            new Vector2Int(1440, 900),
            new Vector2Int(1920, 1200),
            new Vector2Int(2560, 1600)
        };

        #endregion

        #region _____________________________| UNITY

        private void Awake() => _Dropdown ??= GetComponentInChildren<TMP_Dropdown>();

        private void OnEnable()
        {
            if (_Dropdown != null)
                _Dropdown.value = GetCurrentResolutionIndex();
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

        private int GetCurrentResolutionIndex()
        {
            Vector2Int lCurrentResolution = new(Screen.width, Screen.height);
            int lIndex = _SupportedResolutions.IndexOf(lCurrentResolution);
            return Mathf.Clamp(lIndex < 0 ? 0 : lIndex, 0, _SupportedResolutions.Count - 1);
        }

        private void Apply(int pIndex)
        {
            if (_SupportedResolutions.Count == 0)
                return;

            int lClampedIndex = Mathf.Clamp(pIndex, 0, _SupportedResolutions.Count - 1);
            Vector2Int lTarget = _SupportedResolutions[lClampedIndex];
            Screen.SetResolution(lTarget.x, lTarget.y, Screen.fullScreenMode, Screen.currentResolution.refreshRate);
        }

        #endregion
    }
}