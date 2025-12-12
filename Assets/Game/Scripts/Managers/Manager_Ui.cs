#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Singleton
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

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
        [SerializeField] private Transform _WinScreen, _LoseScreen;
        [SerializeField] private List<Transform> _UiCards = new();
        [Header("Cards")]
        [SerializeField] private Transform _SetupPhaseCard;
        private readonly Dictionary<Transform, Transform> _UiCardInstances = new();
        private Transform _CurrentCard;

        [SerializeField] Manager_Game lManager;

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

        public void Show(Transform pCard)
        {
            if (pCard == null) return;

            Transform lCard = AddCardToScene(pCard);
            if (lCard == null) return;

            HideOtherCards(lCard);
            if (IsSetupPhaseCard(lCard))
                ResetGameSpeed();
            _CurrentCard = lCard;
            lCard.gameObject.SetActive(true);
        }

        private void HideOtherCards(Transform pCardToKeep)
        {
            if (pCardToKeep == null)
                return;

            HashSet<Transform> lAllCards = new(_UiCards);
            foreach (Transform lRegistered in _UiCardInstances.Values)
                lAllCards.Add(lRegistered);

            foreach (Transform lCard in lAllCards)
            {
                if (lCard == null)
                    continue;

                if (lCard == pCardToKeep)
                    continue;

                lCard.gameObject.SetActive(false);
            }
        }

        public void Hide(Transform pCard)
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

            lCard.gameObject.SetActive(false);
            if (_CurrentCard == lCard)
                _CurrentCard = null;
        }

        public void Switch(Transform pCardToShow, Transform pCardToHide)
        {
            Hide(pCardToHide);
            Show(pCardToShow);
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


        #endregion
    }
}