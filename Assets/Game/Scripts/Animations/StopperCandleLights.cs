#region _____________________________/ INFOS
//  AUTHOR : Rush Team
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Rush.Game
{
    public class StopperCandleLights : MonoBehaviour
    {
        [Header("Candles")]
        [SerializeField] private List<Light> _CandleLights = new();
        [SerializeField] private bool _CollectLightsOnEnable = true;
        [SerializeField] private Transform _SearchRoot;

        [Header("Flicker Settings")]
        [SerializeField] private Vector2 _IntensityRange = new(2.8f, 4.2f);
        [SerializeField] private Vector2 _RangeRange = new(1.4f, 1.9f);
        [SerializeField] private Vector2 _FlickerDurationRange = new(0.1f, 0.3f);
        [SerializeField] private Vector2 _InitialDelayRange = new(0f, 0.2f);
        [SerializeField] private Ease _FlickerEase = Ease.InOutSine;

        private readonly List<Tween> _ActiveTweens = new();

        private void OnEnable()
        {
            if (_CollectLightsOnEnable)
                CollectLights();

            StartFlicker();
        }

        private void OnDisable()
        {
            StopTweens();
        }

        private void OnDestroy()
        {
            StopTweens();
        }

        private void CollectLights()
        {
            Transform lRoot = _SearchRoot != null ? _SearchRoot : transform;

            _CandleLights.Clear();
            _CandleLights.AddRange(lRoot.GetComponentsInChildren<Light>(includeInactive: true));
        }

        private void StartFlicker()
        {
            foreach (Light lLight in _CandleLights)
            {
                if (lLight == null) continue;

                float lInitialDelay = Random.Range(_InitialDelayRange.x, _InitialDelayRange.y);
                PlayFlickerTween(lLight, lInitialDelay);
            }
        }

        private void PlayFlickerTween(Light pLight, float pDelay = 0f)
        {
            if (pLight == null) return;

            float lDuration = Random.Range(_FlickerDurationRange.x, _FlickerDurationRange.y);
            float lTargetIntensity = Random.Range(_IntensityRange.x, _IntensityRange.y);
            float lTargetRange = Random.Range(_RangeRange.x, _RangeRange.y);

            Sequence lSequence = DOTween.Sequence();

            lSequence.SetDelay(pDelay);
            lSequence.Join(pLight
                .DOIntensity(lTargetIntensity, lDuration)
                .SetEase(_FlickerEase));
            lSequence.Join(DOTween
                .To(() => pLight.range, lValue => pLight.range = lValue, lTargetRange, lDuration)
                .SetEase(_FlickerEase));

            lSequence.OnComplete(() => PlayFlickerTween(pLight));
            lSequence.OnKill(() => _ActiveTweens.Remove(lSequence));

            _ActiveTweens.Add(lSequence);
        }

        private void StopTweens()
        {
            if (_ActiveTweens.Count == 0) return;

            List<Tween> lTweens = new(_ActiveTweens);
            _ActiveTweens.Clear();

            foreach (Tween lTween in lTweens)
            {
                lTween?.Kill();
            }
        }
    }
}