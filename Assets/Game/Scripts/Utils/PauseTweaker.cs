using Rush.Game.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Rush.Game.Utils
{
    public class PauseTweaker : MonoBehaviour
    {
        [SerializeField] private Button _Tweaker;

        void Start()
        {
            if (_Tweaker == null) _Tweaker = GetComponent<Button>();
            if (_Tweaker == null) return;

            _Tweaker.onClick.AddListener(TweakPause);
        }

        void TweakPause()
        {
            bool lWasPaused = Manager_Time.Instance.GetPauseStatus();

            if (lWasPaused)
                Manager_Game.Instance?.StoreInventoryStatusAtGameStart();

            Manager_Time.Instance.SetPauseStatus(!lWasPaused);
        }    }
}
