#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Singleton
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Rush.Game.Core;
using UnityEngine;

namespace Rush.Game
{
    public class Manager_Game : MonoBehaviour
    {
        #region _____________________________/ SINGLETON

        public static Manager_Game Instance { get; private set; }

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
        [SerializeField, Min(0f)] private float _GameWonDelayInSeconds = 5f;
        private bool _HasTriggeredGameOver;
        private bool _HasTriggeredGameWon;
        [SerializeField] private TilePlacer _TilePlacer;
        private List<SO_LevelData.InventoryTile> _InventoryAtGameStart;
        public event Action onGameOver;
        public event Action onGameWon;
        public event Action onGameStart;
        public event Action onGameWonSequenceStarted;
        [Header("Audio")]
        [SerializeField] private AudioClip _WinClip;
        [SerializeField] private string _WinBus = "SFX";
        public event Action onGameRetry;
        [Header("Win Animation")]
        [SerializeField, Min(0f)] private float _LevelAscendHeight = 15f;
        [SerializeField, Min(0f)] private float _LevelAscendDuration = 3f;
        [SerializeField] private Ease _LevelAscendEase = Ease.OutCubic;
        [SerializeField, Min(0f)] private float _LevelAscendDelayInSeconds = 0f;
        private Tween _CurrentLevelWinTween;
        #region _____________________________/ LEVEL DATA

        public SO_LevelData CurrentLevel { get; private set; }

        private int _CubesToComplete;
        private int _CubesArrived;

        public GameObject _CurrentLevelPrefab;

        #endregion

        #region _____________________________| INIT

        private void Awake() => CheckForInstance();

        #endregion

        public void UpdateCubesAmountoComplete(int pAmount) => _CubesToComplete += pAmount;

        public void UpdateCubeArrived()
        {
            _CubesArrived++;
            if (_CubesArrived >= _CubesToComplete && !_HasTriggeredGameWon)
            {
                StartCoroutine(GameWonAfterDelay());
            }
        }
        
        public void GameOver(Cube pCube)
        {
            if (_HasTriggeredGameOver)
                return;

            StartCoroutine(HandleGameOverSequence(pCube));
        }

        private IEnumerator HandleGameOverSequence(Cube pCube)
        {
            _HasTriggeredGameOver = true;
            onGameOver?.Invoke();
            Tween lDeathTween = pCube?.GetValidationTween();
            if (lDeathTween != null && lDeathTween.IsActive())
                yield return lDeathTween.WaitForCompletion();

            Manager_Time.Instance.SetPauseStatus(true);
        }
        
        private System.Collections.IEnumerator GameWonAfterDelay()
        {
            _HasTriggeredGameWon = true;
            InvokeGameWonSequenceStarted();
            if (_LevelAscendDelayInSeconds > 0f)
            {
                yield return new WaitForSeconds(_LevelAscendDelayInSeconds);
            }
            PlayLevelAscendTween();
            PlayWinSound();
            if (_GameWonDelayInSeconds > 0f)
            {
                yield return new WaitForSeconds(_GameWonDelayInSeconds);
            }
            onGameWon?.Invoke();
            Manager_Time.Instance.SetPauseStatus(true);
        }
        #region _____________________________/ LEVEL DATA

        public void SpawnCurrentLevel(SO_LevelData pLevelData)
        {
            _InventoryAtGameStart = null;

            _CubesToComplete = 0;
            _CubesArrived = 0;
            _HasTriggeredGameWon = false;
                        _HasTriggeredGameOver = false;
            CurrentLevel = pLevelData;
            _CurrentLevelPrefab = Instantiate(CurrentLevel.levelPrefab, Vector3.zero, Quaternion.identity);
        }
        public void StoreInventoryStatusAtGameStart()
        {
            _InventoryAtGameStart = UI_Btn_InventoryTile.GetInventorySnapshot();
        }

        public IReadOnlyList<SO_LevelData.InventoryTile> GetInventoryStatusAtGameStart()
        {
            return _InventoryAtGameStart;
        }

        public void UnloadCurrentLevel(bool pReload = false)
        {
                        _CurrentLevelWinTween?.Kill();
            _CurrentLevelWinTween = null;
            Destroy(_CurrentLevelPrefab);
        }

        public void Retry()
        {
            _CubesToComplete = 0;
            _CubesArrived = 0;
            _HasTriggeredGameOver = false;
            _CurrentLevelWinTween?.Kill();
            _CurrentLevelWinTween = null;
            onGameRetry.Invoke();
        }

        public void StartGame()
        {
            onGameStart.Invoke();
        }
        private void PlayWinSound()
        {
            if (_WinClip == null || Manager_Audio.Instance == null)
                return;

            Manager_Audio.Instance.PlayOneShot(_WinClip, pMixerGroup: _WinBus);
        }

        private void InvokeGameWonSequenceStarted()
        {
            try
            {
                onGameWonSequenceStarted?.Invoke();
            }
            catch (Exception lException)
            {
                Debug.LogError($"Exception while notifying win sequence start: {lException}");
            }
        }

                private Tween PlayLevelAscendTween()
        {
            if (_CurrentLevelPrefab == null)
                return null;

            Transform lLevelTransform = _CurrentLevelPrefab.transform;
            _CurrentLevelWinTween?.Kill();
            _CurrentLevelWinTween = lLevelTransform
                .DOMoveY(lLevelTransform.position.y + _LevelAscendHeight, _LevelAscendDuration)
                .SetEase(_LevelAscendEase);

            return _CurrentLevelWinTween;
        }
        #endregion

        #region _____________________________/ DESTROY

        private void OnDestroy() {
            if (Instance == this) Instance = null; }

        #endregion
    }
}