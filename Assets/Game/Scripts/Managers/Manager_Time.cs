#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Singleton
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using UnityEngine;
using System.Collections.Generic;
using System;

namespace Rush.Game.Core
{
    public class Manager_Time : MonoBehaviour
    {
        #region _____________________________/ SINGLETON

        public static Manager_Time Instance { get; private set; }

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

        #region _____________________________/ TICK VALUES
        [Header("Speed Values")]
        [SerializeField, Range(1f, 20f)] private float _MaxSpeed = 20f;

        private float   _GlobalTickSpeed = 1f;
        public float    GlobalTickSpeed { get => _GlobalTickSpeed; set => _GlobalTickSpeed = Mathf.Clamp(value, 0f, _MaxSpeed); }

        private float   _TickDuration = 1f;
        public int     _TickIndex = 0;
        
        private float   _ElapsedTime = 0f;
        private float   _CurrentTickRatio = 0f;

        public event Action<int> onTickFinished;

        #endregion

        #region _____________________________/ MISC VALUES
        private bool _Pause = true;

        public List<ITickDependant> objectsAffectedByTime = new List<ITickDependant>();

        #endregion

        #region _____________________________| INIT

        private void Awake() => CheckForInstance();

        private void Start() => _Pause = true;

        #endregion

        #region _____________________________| UPDATE

        private void Update()
        {
            if (_Pause) return;

            _ElapsedTime += Time.deltaTime * _GlobalTickSpeed;

            while (_ElapsedTime >= _TickDuration)
            {
                _CurrentTickRatio = 1f;
                AdministrateTime();

                _ElapsedTime -= _TickDuration;
                _TickIndex++;
                onTickFinished?.Invoke(_TickIndex);
            }
            
            _CurrentTickRatio = _ElapsedTime / _TickDuration;

            AdministrateTime();
        }

        private void AdministrateTime() { 
            foreach (ITickDependant lObject in objectsAffectedByTime) lObject.currentTickStep = _CurrentTickRatio; }

        #endregion

        public void SetPauseStatus(bool pPause ) => _Pause = pPause;

        public bool GetPauseStatus() => _Pause;

        #region _____________________________| DESTROY

        private void OnDestroy() { 
            if (Instance == this) Instance = null; }
            
        #endregion
    }
}