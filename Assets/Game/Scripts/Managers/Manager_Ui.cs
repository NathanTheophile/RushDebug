#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Singleton
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Rush.Game.Core
{
    public class Manager_Ui : MonoBehaviour
    {
        #region _____________________________/ SINGLETON

        public static Manager_Ui Instance { get; private set; }

        private void CheckForInstance()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;  
        }

        #endregion

        #region _____________________________/ VALUES
        [SerializeField] private Transform _MainCanvas;
        [SerializeField] private Transform _WinScreen, _LoseScreen;
        [SerializeField] private List<Transform> _UiCards = new();
        [Header("Cards")]
        [SerializeField] private Transform _SetupPhaseCard;
        private readonly Dictionary<Transform, Transform> _UiCardInstances = new();
        private Transform _CurrentCard;
        [Header("Fade")]
        [SerializeField] private float _FadeDuration = 1f;
        [SerializeField] private Transform _FullBlackPanel;        private readonly Dictionary<Transform, Coroutine> _FadeRoutines = new();
        [SerializeField] Manager_Game lManager;
                private readonly Dictionary<Transform, Tween> _FadeTweens = new();
        private Coroutine _ShowRoutine;
        #endregion

        #region _____________________________| INIT
        private void Awake()
        {
            CheckForInstance();

            if (_SetupPhaseCard == null)
                _SetupPhaseCard = _UiCards.Find(pCard => pCard != null && pCard.name.Contains("SetupPhase"));
        }

        /// <summary>
        /// Start is called on the frame when a script is enabled just before
        /// any of the Update methods is called the first time.
        /// </summary>
        void Start()
        {
            Manager_Game.Instance.onGameOver += SwitchToLose;
            Manager_Game.Instance.onGameWon += SwitchToWin;
        }

        #endregion

        #region _____________________________| CARDS

        public Transform AddCardToScene(Transform pCard)
        {
            if (pCard == null)
                return null;

            if (_UiCardInstances.TryGetValue(pCard, out Transform lCard))
            {
                if (!_UiCards.Contains(lCard))
                    _UiCards.Add(lCard);

                return lCard;
            }

            // If the card is already part of the scene (not a prefab asset), just register it.
            if (pCard.gameObject.scene.IsValid())
            {
                _UiCardInstances[pCard] = pCard;
                if (!_UiCards.Contains(pCard))
                    _UiCards.Add(pCard);

                return pCard;
            }

            lCard = Instantiate(pCard, _MainCanvas);
            _UiCardInstances[pCard] = lCard;
            _UiCards.Add(lCard);

            return lCard;
        }

        public void Show(Transform pCard, bool pFadeBlack = false)
        {
            if (pCard == null) return;

            Transform lCard = AddCardToScene(pCard);
            if (lCard == null) return;

            if (_ShowRoutine != null)
                StopCoroutine(_ShowRoutine);

            _ShowRoutine = StartCoroutine(ShowAfterFadeOut(lCard, pFadeBlack));
        }

        private IEnumerator ShowAfterFadeOut(Transform pCard, bool pFadeBlack)
        {
            List<Transform> lOtherCards = CollectOtherCards(pCard);

            if (IsSetupPhaseCard(pCard))
                ResetGameSpeed();

            _CurrentCard = pCard;

            if (pFadeBlack)
            {
                yield return FadeBlack(true);
                HideCardsImmediate(lOtherCards);
            }
            else
            {
                List<Transform> lCardsToWait = FadeOutCards(lOtherCards);
                yield return WaitForFadeOuts(lCardsToWait);
            }

            FadeInCard(pCard);

            if (pFadeBlack)
                yield return FadeBlack(false);

            _ShowRoutine = null;
        }

        private List<Transform> CollectOtherCards(Transform pCardToKeep)
        {
            if (pCardToKeep == null)
                return new List<Transform>();

            HashSet<Transform> lAllCards = new(_UiCards);
            foreach (Transform lRegistered in _UiCardInstances.Values)
                lAllCards.Add(lRegistered);

            List<Transform> lCardsToFade = new();
            foreach (Transform lCard in lAllCards)
            {
                if (lCard == null)
                    continue;

                if (lCard == pCardToKeep)
                    continue;

                lCardsToFade.Add(lCard);
            }
            return lCardsToFade;
        }

        private List<Transform> FadeOutCards(List<Transform> pCards)
        {
            List<Transform> lCardsToFade = new();
            if (pCards == null)
                return lCardsToFade;

            foreach (Transform lCard in pCards)
            {
                FadeOutCard(lCard);
                if (lCard != null)
                    lCardsToFade.Add(lCard);
            }

            return lCardsToFade;
        }

        private void HideCardsImmediate(IEnumerable<Transform> pCards)
        {
            if (pCards == null)
                return;

            foreach (Transform lCard in pCards)
            {
                if (lCard == null)
                    continue;

                CancelFade(lCard, false);
                CanvasGroup lGroup = EnsureCanvasGroup(lCard);
                lGroup.alpha = 0f;
                lCard.gameObject.SetActive(false);
                _FadeTweens.Remove(lCard);            }
        }

        public void Hide(Transform pCard, bool pFadeBlack = false)
        {
            if (pCard == null) return;

            Transform lCard = null;

            if (_UiCardInstances.TryGetValue(pCard, out Transform lExisting))
            {
                lCard = lExisting;
            }
            else if (_UiCardInstances.ContainsValue(pCard))
            {
                lCard = pCard;
            }
            else if (pCard.gameObject.scene.IsValid())
            {
                // Register existing scene objects so they can be hidden without spawning duplicates.
                lCard = pCard;
                _UiCardInstances[pCard] = pCard;
                if (!_UiCards.Contains(pCard))
                    _UiCards.Add(pCard);
            }

            if (lCard == null) return;

            if (pFadeBlack && _FullBlackPanel != null)            {
                StartCoroutine(FadeBlackHideRoutine(lCard));
            }
            else
            {
                FadeOutCard(lCard);
            }

            if (_CurrentCard == lCard)
                _CurrentCard = null;
        }

        public void Switch(Transform pCardToShow, Transform pCardToHide, bool pFadeBlack = false)
        {
            Hide(pCardToHide, pFadeBlack);
            Show(pCardToShow, pFadeBlack);
        }

        private void SwitchToWin()  { Switch(_WinScreen, _CurrentCard);}        
        private void SwitchToLose() { Switch(_LoseScreen, _CurrentCard);}


        private bool IsSetupPhaseCard(Transform pCard)
        {
            if (pCard == null || _SetupPhaseCard == null)
                return false;

            if (pCard == _SetupPhaseCard)
                return true;

            if (_UiCardInstances.TryGetValue(_SetupPhaseCard, out Transform lInstance))
                return pCard == lInstance;

            return false;
        }

        private void ResetGameSpeed() => Manager_Time.Instance.GlobalTickSpeed = 1f;

        private void FadeInCard(Transform pCard)
        {
            if (pCard == null)
                return;

            CanvasGroup lGroup = EnsureCanvasGroup(pCard);
            if (!pCard.gameObject.activeSelf)
            {
                lGroup.alpha = 0f;
                pCard.gameObject.SetActive(true);
            }

            StartFadeRoutine(pCard, lGroup, 1f, false);
        }

        private void FadeOutCard(Transform pCard)
        {
            if (pCard == null)
                return;

            CanvasGroup lGroup = EnsureCanvasGroup(pCard);
            StartFadeRoutine(pCard, lGroup, 0f, true);
        }

        private IEnumerator WaitForFadeOuts(List<Transform> pCards)
        {
            if (pCards == null || pCards.Count == 0)
                yield break;

            bool lWaiting = true;
            while (lWaiting)
            {
                lWaiting = false;
                foreach (Transform lCard in pCards)
                {
                    if (lCard != null && _FadeTweens.TryGetValue(lCard, out Tween lTween) && lTween.IsActive() && lTween.IsPlaying())                    {
                        lWaiting = true;
                        break;
                    }
                }

                if (lWaiting)
                    yield return null;
            }
        }

        private CanvasGroup EnsureCanvasGroup(Transform pCard)
        {
            CanvasGroup lGroup = pCard.GetComponent<CanvasGroup>();
            if (lGroup == null)
                lGroup = pCard.gameObject.AddComponent<CanvasGroup>();

            return lGroup;
        }

        private void CancelFade(Transform pCard, bool pDeactivateAfter)
        {
            if (pCard == null)
                return;

            if (_FadeTweens.TryGetValue(pCard, out Tween lTween))
                lTween.Kill();

            _FadeTweens.Remove(pCard);

            if (pDeactivateAfter)
                pCard.gameObject.SetActive(false);
        }

        private Tween StartFadeRoutine(Transform pCard, CanvasGroup pGroup, float pTargetAlpha, bool pDeactivateAfter)
        {
            CancelFade(pCard, false);

            if (pCard == null || pGroup == null)
                return null;

            float lDuration = Mathf.Max(0.01f, _FadeDuration);

            if (pTargetAlpha > 0f && !pCard.gameObject.activeSelf)
                pCard.gameObject.SetActive(true);

            pGroup.DOKill();

            Tween lTween = pGroup
                .DOFade(pTargetAlpha, lDuration)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (pDeactivateAfter && Mathf.Approximately(pTargetAlpha, 0f))
                        pCard.gameObject.SetActive(false);

                    _FadeTweens.Remove(pCard);
                });

            _FadeTweens[pCard] = lTween;
            return lTween;
        }

        private IEnumerator FadeBlack(bool pFadeIn)
        {
            if (_FullBlackPanel == null)
                yield break;

            CanvasGroup lGroup = EnsureCanvasGroup(_FullBlackPanel);

            if (pFadeIn && !_FullBlackPanel.gameObject.activeSelf)
            {
                lGroup.alpha = 0f;
                _FullBlackPanel.gameObject.SetActive(true);
            }

            float lTarget = pFadeIn ? 1f : 0f;
            bool lDeactivate = !pFadeIn;
            Tween lTween = StartFadeRoutine(_FullBlackPanel, lGroup, lTarget, lDeactivate);

            if (lTween != null)
                yield return lTween.WaitForCompletion();
        }

        private IEnumerator FadeBlackHideRoutine(Transform pCard)
        {
            yield return FadeBlack(true);

            CancelFade(pCard, false);
            CanvasGroup lGroup = EnsureCanvasGroup(pCard);
            lGroup.alpha = 0f;
            pCard.gameObject.SetActive(false);
            _FadeTweens.Remove(pCard);

            yield return FadeBlack(false);
        }
        #endregion
    }
}