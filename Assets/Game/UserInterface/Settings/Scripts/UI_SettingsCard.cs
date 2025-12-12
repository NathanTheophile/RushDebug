using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rush.UI
{
    public class UI_SettingsCard : MonoBehaviour
    {
        #region _____________________________/ VALUES

        [SerializeField] private List<Button> _TabButtons = new();
        [SerializeField] private List<GameObject> _Tabs = new();

        private int _CurrentTabIndex = -1;

        #endregion

        #region _____________________________| UNITY

        private void Awake()
        {
            for (int lIndex = 0; lIndex < _TabButtons.Count; lIndex++)
            {
                int lCachedIndex = lIndex;
                Button lButton = _TabButtons[lIndex];
                if (lButton != null)
                    lButton.onClick.AddListener(() => SwitchToTab(lCachedIndex));
            }
        }

        private void OnEnable()
        {
            if (_Tabs.Count == 0)
                return;

            SwitchToTab(Mathf.Clamp(_CurrentTabIndex, 0, _Tabs.Count - 1));
        }

        private void Start()
        {
            if (_Tabs.Count > 0 && _CurrentTabIndex < 0)
                SwitchToTab(0);
        }

        #endregion

        #region _____________________________| METHODS

        private void SwitchToTab(int pIndex)
        {
            if (pIndex < 0 || pIndex >= _Tabs.Count)
                return;

            if (_CurrentTabIndex >= 0 && _CurrentTabIndex < _Tabs.Count)
            {
                GameObject lCurrent = _Tabs[_CurrentTabIndex];
                if (lCurrent != null)
                    lCurrent.SetActive(false);
            }

            GameObject lNext = _Tabs[pIndex];
            if (lNext != null)
                lNext.SetActive(true);

            _CurrentTabIndex = pIndex;
        }

        #endregion
    }
}