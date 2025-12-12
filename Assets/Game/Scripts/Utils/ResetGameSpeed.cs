using Rush.Game.Core;
using UnityEngine;
using UnityEngine.UI;

public class ResetGameSpeed : MonoBehaviour
{
        [SerializeField] private Button _Tweaker;

        void Start()
        {
            if (_Tweaker == null) _Tweaker = GetComponent<Button>();
            if (_Tweaker == null) return;

            _Tweaker.onClick.AddListener(TweakSpeed);
        }

        void TweakSpeed()
        {
            Manager_Time.Instance.GlobalTickSpeed = 1f;
        }
}
