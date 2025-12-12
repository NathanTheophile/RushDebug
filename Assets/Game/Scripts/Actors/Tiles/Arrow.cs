#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using Rush.Game.Core;
using UnityEngine;

namespace Rush.Game
{
    public class Arrow : Tile
    {
        private const string ARROW_CHILD_NAME = "Arrow";
        private const string OPEN_CLOSE_STATE_NAME = "Armature|OpenClose";
        private const string PUSH_STATE_NAME = "Armature|Push";

        private Animator m_ArrowAnimator;
        private bool m_WasPaused;

        private void Awake()
        {
            CacheAnimator();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            base.Start();
            m_WasPaused = Manager_Time.Instance?.GetPauseStatus() ?? true;
        }

        private void CacheAnimator()
        {
            Transform lArrow = transform.Find(ARROW_CHILD_NAME);

            if (lArrow == null)
                return;

            lArrow.TryGetComponent(out m_ArrowAnimator);
        }

        public void PlayPlacementAnimation()
        {
            if (m_ArrowAnimator == null)
            {
                CacheAnimator();

                if (m_ArrowAnimator == null)
                    return;
            }

            m_ArrowAnimator.Play(OPEN_CLOSE_STATE_NAME, 0, 0f);
            UpdateAnimatorSpeed();
        }

        private void Update()
        {
            UpdateAnimatorSpeed();
            HandlePauseChange();
        }

        private void HandlePauseChange()
        {
            bool lIsPaused = Manager_Time.Instance?.GetPauseStatus() ?? true;

            if (m_WasPaused && !lIsPaused)
                PlayPushAnimation();

            m_WasPaused = lIsPaused;
        }

        private void PlayPushAnimation()
        {
            if (m_ArrowAnimator == null)
                return;

            m_ArrowAnimator.Play(PUSH_STATE_NAME, 0, 0f);
            UpdateAnimatorSpeed();
        }

        private void UpdateAnimatorSpeed()
        {
            if (m_ArrowAnimator == null)
                return;

            AnimatorStateInfo lStateInfo = m_ArrowAnimator.GetCurrentAnimatorStateInfo(0);

            if (lStateInfo.IsName(PUSH_STATE_NAME))
            {
                float lSpeed = 0f;

                if (Manager_Time.Instance != null && !Manager_Time.Instance.GetPauseStatus())
                    lSpeed = Manager_Time.Instance.GlobalTickSpeed / 2f;

                m_ArrowAnimator.speed = lSpeed;
            }
            else
            {
                m_ArrowAnimator.speed = 1f;
            }
        }
    }
}