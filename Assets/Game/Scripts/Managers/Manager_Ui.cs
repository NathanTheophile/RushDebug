#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Singleton
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private Transform _WinScreen, _LoseScreen, MainMenu;
        [SerializeField] private List<Transform> _UiCards = new();
        [Header("Cards")]
        [SerializeField] private Transform _SetupPhaseCard;
        [Header("Fade")]
        [SerializeField] private float _FadeDuration = 0.25f;
        [SerializeField] private CanvasGroup _FullBlackFade;
        private bool _IsBlackFading;
        private bool _IsBlackFadingIn;
        private readonly Dictionary<Transform, Coroutine> _FadeRoutines = new();
        private readonly Dictionary<Transform, Transform> _UiCardInstances = new();
        private Transform _CurrentCard;
        private Coroutine _ShowRoutine;

        [SerializeField] Manager_Game lManager;

                [Header("Inputs")]
        [SerializeField] private KeyCode _ToggleCurrentCardKey = KeyCode.O;
        private bool _IsCurrentCardHidden;

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
            Show(MainMenu);
        }

        void Update()
        {
            HandleCurrentCardToggleInput();
        }
        #endregion

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

                        Transform lExistingInstance = FindExistingCardInstance(pCard);
            if (lExistingInstance != null)
            {
                _UiCardInstances[pCard] = lExistingInstance;
                if (!_UiCards.Contains(lExistingInstance))
                    _UiCards.Add(lExistingInstance);

                return lExistingInstance;
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


        private Transform FindExistingCardInstance(Transform pCard)
        {
            if (_MainCanvas == null || pCard == null)
                return null;

            foreach (Transform lChild in _MainCanvas.GetComponentsInChildren<Transform>(true))
            {
                if (lChild == null || lChild == pCard)
                    continue;

                if (_UiCardInstances.ContainsValue(lChild))
                    continue;

                if (lChild.name == pCard.name)
                    return lChild;
            }

            return null;
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

            if (pFadeBlack && pCard != null && pCard.gameObject.activeSelf)
            {
                CanvasGroup lTargetGroup = EnsureCanvasGroup(pCard);
                CancelFade(pCard, false);
                lTargetGroup.alpha = 0f;
                pCard.gameObject.SetActive(false);
            }

            if (IsSetupPhaseCard(pCard))
                ResetGameSpeed();

            SetCurrentCard(pCard);

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
                _FadeRoutines.Remove(lCard);
            }
        }

        public void Hide(Transform pCard, bool pFadeBlack = false, bool pDeactivateHiddenCardInstantly = false)
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

            if (pFadeBlack && _FullBlackFade != null)
            {
                StartCoroutine(FadeBlackHideRoutine(lCard, pDeactivateHiddenCardInstantly));
            }
            else
            {
                FadeOutCard(lCard, pDeactivateHiddenCardInstantly);
            }

            if (_CurrentCard == lCard)
            {
                _CurrentCard = null;
                _IsCurrentCardHidden = false;
            }
        }

        public void Switch(Transform pCardToShow, Transform pCardToHide, bool pFadeBlack = false, bool pDeactivateHiddenCardInstantly = false)
        {
            Hide(pCardToHide, pFadeBlack, pDeactivateHiddenCardInstantly);
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

        private void FadeOutCard(Transform pCard, bool pDeactivateImmediately = false)
        {
            if (pCard == null)
                return;

            CanvasGroup lGroup = EnsureCanvasGroup(pCard);

            if (pDeactivateImmediately)
            {
                lGroup.alpha = 0f;
                pCard.gameObject.SetActive(false);
                CancelFade(pCard, false);
                _FadeRoutines.Remove(pCard);
                StartFadeRoutine(pCard, lGroup, 0f, false, true);
                return;
            }

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
                    if (lCard != null && _FadeRoutines.ContainsKey(lCard))
                    {
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
            if (_FadeRoutines.TryGetValue(pCard, out Coroutine lRoutine))
                StopCoroutine(lRoutine);

            _FadeRoutines.Remove(pCard);

            if (pDeactivateAfter && pCard != null)
                pCard.gameObject.SetActive(false);
        }

        private Coroutine StartFadeRoutine(Transform pCard, CanvasGroup pGroup, float pTargetAlpha, bool pDeactivateAfter, bool pDeactivateOnStart = false)
        {
            CancelFade(pCard, false);

            Coroutine lRoutine = StartCoroutine(FadeRoutine(pCard, pGroup, pTargetAlpha, pDeactivateAfter, pDeactivateOnStart));
            _FadeRoutines[pCard] = lRoutine;
            return lRoutine;
        }

        private IEnumerator FadeRoutine(Transform pCard, CanvasGroup pGroup, float pTargetAlpha, bool pDeactivateAfter, bool pDeactivateOnStart)
        {
            float lDuration = Mathf.Max(0.01f, _FadeDuration);
            float lStartAlpha = pGroup.alpha;
            float lTimer = 0f;

            if (pDeactivateOnStart)
                pCard.gameObject.SetActive(false);

            while (lTimer < lDuration)
            {
                lTimer += Time.unscaledDeltaTime;
                float lProgress = Mathf.Clamp01(lTimer / lDuration);
                pGroup.alpha = Mathf.Lerp(lStartAlpha, pTargetAlpha, lProgress);
                yield return null;
            }

            pGroup.alpha = pTargetAlpha;

            if (pDeactivateAfter && Mathf.Approximately(pTargetAlpha, 0f))
                pCard.gameObject.SetActive(false);

            _FadeRoutines.Remove(pCard);
        }

        private IEnumerator FadeBlack(bool pFadeIn)
        {
            if (_FullBlackFade == null)
                yield break;

            BringFullBlackFadeToFront();

            _IsBlackFading = true;
            _IsBlackFadingIn = pFadeIn;

            if (pFadeIn && !_FullBlackFade.gameObject.activeSelf)
            {
                _FullBlackFade.alpha = 0f;
                _FullBlackFade.gameObject.SetActive(true);
            }

            float lTarget = pFadeIn ? 1f : 0f;
            bool lDeactivate = !pFadeIn;
            Coroutine lRoutine = StartFadeRoutine(_FullBlackFade.transform, _FullBlackFade, lTarget, lDeactivate);

            if (lRoutine != null)
                yield return lRoutine;

            _IsBlackFading = false;
            _IsBlackFadingIn = false;
        }

        private void BringFullBlackFadeToFront()
        {
            if (_FullBlackFade == null)
                return;

            _FullBlackFade.transform.SetAsLastSibling();

            Canvas lCanvas = _FullBlackFade.GetComponent<Canvas>();
            if (lCanvas != null)
            {
                lCanvas.overrideSorting = true;
                lCanvas.sortingOrder = short.MaxValue;
            }
        }

        private IEnumerator FadeBlackHideRoutine(Transform pCard, bool pDeactivateOnStart)
        {
            if (pDeactivateOnStart)
            {
                CancelFade(pCard, false);
                CanvasGroup lPreGroup = EnsureCanvasGroup(pCard);
                lPreGroup.alpha = 0f;
                pCard.gameObject.SetActive(false);
                _FadeRoutines.Remove(pCard);
            }

            yield return FadeBlack(true);

            if (!pDeactivateOnStart)
            {
                CancelFade(pCard, false);
                CanvasGroup lGroup = EnsureCanvasGroup(pCard);
                lGroup.alpha = 0f;
                pCard.gameObject.SetActive(false);
                _FadeRoutines.Remove(pCard);
            }

            yield return FadeBlack(false);
        }

        public IEnumerator WaitForBlackMidpoint()
        {
            if (_FullBlackFade == null)
                yield break;

            while (!_FullBlackFade.gameObject.activeSelf && !_IsBlackFading)
                yield return null;

            while (_IsBlackFading && !_IsBlackFadingIn)
                yield return null;

            while (_FullBlackFade.gameObject.activeSelf && _FullBlackFade.alpha < 0.99f)
                yield return null;
        }

                private void HandleCurrentCardToggleInput()
        {
            if (_ToggleCurrentCardKey == KeyCode.None)
                return;

            if (!Input.GetKeyDown(_ToggleCurrentCardKey))
                return;

            ToggleCurrentCardVisibility();
        }

        private void ToggleCurrentCardVisibility()
        {
            if (_CurrentCard == null)
                return;

            if (_IsCurrentCardHidden)
                ShowCurrentCardImmediate();
            else
                HideCurrentCardImmediate();
        }

        private void HideCurrentCardImmediate()
        {
            if (_CurrentCard == null)
                return;

            CancelFade(_CurrentCard, false);
            _CurrentCard.gameObject.SetActive(false);
            _IsCurrentCardHidden = true;
        }

        private void ShowCurrentCardImmediate()
        {
            if (_CurrentCard == null)
                return;

            CancelFade(_CurrentCard, false);
            _CurrentCard.gameObject.SetActive(true);
            CanvasGroup lGroup = EnsureCanvasGroup(_CurrentCard);
            lGroup.alpha = 1f;
            _IsCurrentCardHidden = false;
        }

        private void SetCurrentCard(Transform pCard)
        {
            _CurrentCard = pCard;
            _IsCurrentCardHidden = false;
        }

    }

}
