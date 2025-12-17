#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using UnityEngine;
using DG.Tweening;
using Rush.Game.Core;

namespace Rush.Game
{
    public class Dispatcher : Tile
    {
        private bool _Switcher = true;

        [Header("Switch Animation")]
        [SerializeField] private Transform _SwitchTarget;
        [SerializeField] private float _SwitchTweenDuration = .25f;

        private Vector3 _InitialLocalRotation;
        private Tween _SwitchTween;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            m_Direction = Vector3.right;
            if (_SwitchTarget != null) _InitialLocalRotation = _SwitchTarget.localEulerAngles;
            Manager_Game.Instance.onGameRetry += ResetDispatcher;
        }

        void ResetDispatcher()
        {
            m_Direction = Vector3.right;
            _Switcher = true;
            if (_SwitchTarget != null)
            {
                _SwitchTween?.Kill();
                _SwitchTarget.localEulerAngles = _InitialLocalRotation;
            }
        }


        public void Switch()
        {
            if (_Switcher) { m_Direction = Vector3.left; _Switcher = false; }
            else { m_Direction = Vector3.right; _Switcher = true; }

            PlaySwitchTween();
        }

        private void PlaySwitchTween()
        {
            if (_SwitchTarget == null) return;

            float lTickSpeed = Manager_Time.Instance != null ? Manager_Time.Instance.GlobalTickSpeed : 1f;
            float lDuration = _SwitchTweenDuration / Mathf.Max(lTickSpeed, Mathf.Epsilon);

            _SwitchTween?.Kill();
            _SwitchTween = _SwitchTarget
                .DOLocalRotate(new Vector3(0f, 180f, 0f), lDuration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.OutSine)
                .OnKill(() => _SwitchTween = null);
        }
    }
}