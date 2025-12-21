using TMPro;
using UnityEngine;

namespace Rush.UI
{
    public class UI_Setting_DisplayMode : MonoBehaviour
    {
        #region _____________________________/ VALUES

        [SerializeField] private TMP_Dropdown _Dropdown;

        #endregion

        #region _____________________________| UNITY

        private void Awake() => _Dropdown ??= GetComponentInChildren<TMP_Dropdown>();

        private void OnEnable()
        {
            if (_Dropdown != null)
                _Dropdown.value = ModeToIndex(Screen.fullScreenMode);
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
            FullScreenMode lMode = IndexToMode(pIndex);
            bool lIsFullscreen = lMode == FullScreenMode.ExclusiveFullScreen || lMode == FullScreenMode.FullScreenWindow;

            Screen.fullScreenMode = lMode;
            Screen.fullScreen = lIsFullscreen;
        }

        private static int ModeToIndex(FullScreenMode pMode) => pMode switch
        {
            FullScreenMode.ExclusiveFullScreen => 0,
            FullScreenMode.FullScreenWindow => 1,
            _ => 2
        };

        private static FullScreenMode IndexToMode(int pIndex) => Mathf.Clamp(pIndex, 0, 2) switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.FullScreenWindow,
            _ => FullScreenMode.Windowed
        };

        #endregion
    }
}