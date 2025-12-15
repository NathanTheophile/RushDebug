#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using System;
using DG.Tweening;
using Rush.Game.Core;
using Unity.VisualScripting;
using UnityEngine;
using static Rush.Game.SO_Colors;

namespace Rush.Game
{
    public class Target : Tile
    {
        [SerializeField] private SO_Colors _ColorSO;
        private Color _Color;
        [SerializeField] private ColorsData _ColorData;

        [Header("Light")]
        [SerializeField] private Light _Light;
        [SerializeField, Min(0f)] private float _LightPulseDuration = 1.5f;
        [SerializeField] private Vector2 _LightPulseIntensity = new(0.7f, 1.2f);
        [Header("Audio")]
        [SerializeField] private AudioClip _CubeArrivedClip;
        [SerializeField] private string _CubeArrivedBus = "SFX";
        private Tween _LightTween;
        private float _BaseLightIntensity = 1f;
        [SerializeField] GameObject fleurs;
                [SerializeField] GameObject sol;
        [SerializeField] GameObject fleur;

        private Material _Material;
        private Material _Emissive;
        private Manager_Time     timeManager;
        private Manager_Tile     tileManager;
        private Manager_Game     gameManager;

        public event Action onCubeValidation;

        void Awake()
        {
            _Material = _ColorSO.Material;
            _Emissive = _ColorSO.Emissive;
            _Color = _ColorSO.Color;
            _ColorData = _ColorSO.ColorData;
            var fleursR = fleurs.GetComponent<Renderer>();
            var fleursM = fleursR.materials;
            fleursM[0] = _Material;
            fleursM[1] = _Emissive;
            fleursR.materials = fleursM;
            
            sol.GetComponent<Renderer>().material = _Emissive;

            var fleurR = fleur.GetComponent<Renderer>();
            var fleurM = fleurR.materials;
            fleurM[0] = _Material;
            fleurM[1] = _Emissive;
            fleurR.materials = fleurM;

            if (_Light != null)
            {
                _BaseLightIntensity = _Light.intensity;
                _Light.color = _Color;
                StartLightPulse();
            }
                        
        }
        private void OnDisable()
        {
            _LightTween?.Kill();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start()
        {
            base.Start();
            timeManager = Manager_Time.Instance;
            tileManager = Manager_Tile.Instance;
            gameManager = Manager_Game.Instance;
        }

        private void StartLightPulse()
        {
            _LightTween?.Kill();
            if (_Light == null || _LightPulseDuration <= 0f)
                return;

            float lMinIntensity = _BaseLightIntensity * _LightPulseIntensity.x;
            float lMaxIntensity = _BaseLightIntensity * _LightPulseIntensity.y;
            _Light.intensity = lMinIntensity;

            _LightTween = _Light.DOIntensity(lMaxIntensity, _LightPulseDuration * 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        
        public bool CheckColor(Cube pCube)
        {
            if (pCube.ColorData != _ColorData) return false;
            HandleCubeValidation(pCube);
            onCubeValidation?.Invoke();
            return true;
        }

        private void HandleCubeValidation(Cube pCube)
        {
            pCube.SetModePause();
            timeManager.onTickFinished -= pCube.TickUpdate;
            timeManager.objectsAffectedByTime.Remove(pCube);
            pCube.onTileDetected -= tileManager.TryGetTile;
                        pCube.onCubeDeath -= gameManager.GameOver;
            AudioClip lClipToPlay = gameManager?.GetNextCubeArrivedClip() ?? _CubeArrivedClip;
            if (lClipToPlay != null && Manager_Audio.Instance != null)
                Manager_Audio.Instance.PlayAtPosition(lClipToPlay, transform.position, pMixerGroup: _CubeArrivedBus);            gameManager.UpdateCubeArrived();

            pCube.PlayValidationTween(() => Destroy(pCube.GameObject()));
        }
    }
}