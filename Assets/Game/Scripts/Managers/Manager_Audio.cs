#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Singleton
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Rush.Game.Core
{
    public class Manager_Audio : MonoBehaviour
    {
        #region _____________________________/ SINGLETON

        public static Manager_Audio Instance { get; private set; }

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
        [Header("Pooling")]
        [SerializeField, Range(1, 32)] private int _InitialPoolSize = 8;
        [SerializeField, Range(1, 64)] private int _MaxPoolSize = 32;

        [Header("Defaults")]
        [SerializeField, Range(0f, 1f)] private float _DefaultVolume = 1f;
        [SerializeField] private float _DefaultPitch = 1f;
        [SerializeField] private bool _LoopByDefault = false;
        [SerializeField] private AudioMixer _AudioBus;
        [SerializeField] private string _DefaultMixerGroupName = "Master";
        [SerializeField] private AudioMixerGroup _DefaultMixerGroup;

        [SerializeField] private AudioClip _DefaultMusic;

        private readonly List<PooledSource> _AudioSources = new();
        private readonly Dictionary<string, AudioMixerGroup> _MixerGroupCache = new();
        #endregion

        #region _____________________________/ ACCESSORS

        public AudioMixer AudioBus => _AudioBus;

        #endregion

        #region _____________________________| INIT

        private void Awake()
        {
            CheckForInstance();
            if (Instance != this) return;

            WarmPool(_InitialPoolSize);
            if (_DefaultMixerGroup == null)
                _DefaultMixerGroup = ResolveMixerGroup(_DefaultMixerGroupName);
        }

        #endregion

        #region _____________________________| UPDATE

        private void Update()
        {
            foreach (PooledSource lSource in _AudioSources)
            {
                if (!lSource.inUse)
                    continue;

                if (lSource.followTarget != null)
                    lSource.source.transform.position = lSource.followTarget.position;

                if (!lSource.source.loop && !lSource.source.isPlaying)
                    ReleaseSource(lSource);
            }
        }

        #endregion

        #region _____________________________| PLAY

        public AudioSource PlayOneShot(AudioClip pClip, float pVolume = -1f, string pMixerGroup = "")
        {
            return PlayClip(pClip, false, pVolume, pMixerGroup);
        }

        public AudioSource PlayLoop(AudioClip pClip, float pVolume = -1f, string pMixerGroup = "", float pSpatialBlend = 0f)
        {
            return PlayClip(pClip, true, pVolume, pMixerGroup, pSpatialBlend: pSpatialBlend);
        }

        public AudioSource PlayAtPosition(AudioClip pClip, Vector3 pPosition, bool pLoop = false, float pVolume = -1f, string pMixerGroup = "", float pSpatialBlend = 1f)
        {
            return PlayClip(pClip, pLoop, pVolume, pMixerGroup, pSpatialBlend: pSpatialBlend, pWorldPosition: pPosition);
        }

        public AudioSource PlayFollow(AudioClip pClip, Transform pFollowTarget, bool pLoop = false, float pVolume = -1f, string pMixerGroup = "", float pSpatialBlend = 1f)
        {
            return PlayClip(pClip, pLoop, pVolume, pMixerGroup, pSpatialBlend: pSpatialBlend, pFollowTarget: pFollowTarget);
        }

        public void Stop(AudioSource pSource)
        {
            if (pSource == null)
                return;

            PooledSource lEntry = FindPoolEntry(pSource);
            if (lEntry != null)
                ReleaseSource(lEntry);
            else
                pSource.Stop();
        }

        public void StopAll()
        {
            foreach (PooledSource lEntry in _AudioSources)
                ReleaseSource(lEntry);
        }

        private AudioSource PlayClip(AudioClip pClip, bool pLoop, float pVolume, string pMixerGroup, float pPitch = -1f, float pSpatialBlend = 0f, float pStartTime = 0f, Vector3? pWorldPosition = null, Transform pFollowTarget = null)
        {
            if (pClip == null)
                return null;

            PooledSource lEntry = GetAvailableSource();
            if (lEntry == null)
                return null;

            lEntry.inUse = true;
            lEntry.followTarget = pFollowTarget;

            AudioSource lSource = lEntry.source;

            lSource.clip = pClip;
            lSource.loop = pLoop || _LoopByDefault;
            lSource.volume = pVolume >= 0f ? pVolume : _DefaultVolume;
            lSource.pitch = pPitch >= 0f ? pPitch : _DefaultPitch;
            lSource.spatialBlend = Mathf.Clamp01(pSpatialBlend);
            lSource.time = Mathf.Clamp(pStartTime, 0f, pClip.length);
            lSource.outputAudioMixerGroup = ResolveMixerGroup(string.IsNullOrWhiteSpace(pMixerGroup) ? _DefaultMixerGroupName : pMixerGroup);

            if (pWorldPosition.HasValue)
                lSource.transform.position = pWorldPosition.Value;
            else if (pFollowTarget != null)
                lSource.transform.position = pFollowTarget.position;
            else
                lSource.transform.localPosition = Vector3.zero;

            lSource.Play();
            return lSource;
        }

        #endregion

        #region _____________________________| POOL

        private void WarmPool(int pAmount)
        {
            for (int lIndex = 0; lIndex < pAmount; lIndex++)
                _AudioSources.Add(CreateSource());
        }

        private PooledSource GetAvailableSource()
        {
            foreach (PooledSource lEntry in _AudioSources)
            {
                if (!lEntry.inUse)
                    return lEntry;
            }

            if (_AudioSources.Count >= _MaxPoolSize)
                return null;

            PooledSource lSource = CreateSource();
            _AudioSources.Add(lSource);
            return lSource;
        }

        private PooledSource CreateSource()
        {
            GameObject lHolder = new GameObject($"AudioSource_{_AudioSources.Count}");
            lHolder.transform.SetParent(transform);

            AudioSource lSource = lHolder.AddComponent<AudioSource>();
            lSource.playOnAwake = false;
            lSource.loop = false;
            lSource.volume = _DefaultVolume;
            lSource.pitch = _DefaultPitch;
            lSource.outputAudioMixerGroup = ResolveMixerGroup(_DefaultMixerGroupName);

            return new PooledSource(lSource);
        }

        private void ReleaseSource(PooledSource pSource, bool pStop = true)
        {
            if (pSource == null)
                return;

            if (pStop)
                pSource.source.Stop();

            pSource.source.loop = false;
            pSource.source.spatialBlend = 0f;
            pSource.source.pitch = _DefaultPitch;
            pSource.source.volume = _DefaultVolume;

            if (pSource.source.clip != null)
                pSource.source.time = 0f;

            pSource.source.clip = null;
            pSource.followTarget = null;
            pSource.inUse = false;
        }

        private PooledSource FindPoolEntry(AudioSource pSource)
        {
            foreach (PooledSource lEntry in _AudioSources)
            {
                if (lEntry.source == pSource)
                    return lEntry;
            }

            return null;
        }

        #endregion

        #region _____________________________| MIXER

        private AudioMixerGroup ResolveMixerGroup(string pMixerGroupName)
        {
            if (_DefaultMixerGroup != null && string.IsNullOrWhiteSpace(pMixerGroupName))
                return _DefaultMixerGroup;

            if (_MixerGroupCache.TryGetValue(pMixerGroupName, out AudioMixerGroup lCachedGroup))
                return lCachedGroup;

            if (_AudioBus != null)
            {
                AudioMixerGroup[] lGroups = _AudioBus.FindMatchingGroups(pMixerGroupName);
                if (lGroups != null && lGroups.Length > 0)
                {
                    _MixerGroupCache[pMixerGroupName] = lGroups[0];
                    return lGroups[0];
                }
            }

            return _DefaultMixerGroup;
        }

        #endregion

        #region _____________________________| MIXER VOLUMES

        public float GetGroupVolume(string pGroupName, float pFallback = 1f)
        {
            if (string.IsNullOrWhiteSpace(pGroupName) || _AudioBus == null)
                return pFallback;

            if (_AudioBus.GetFloat($"{pGroupName}Volume", out float lVolumeDb))
                return Mathf.Pow(10f, lVolumeDb / 20f);

            return pFallback;
        }

        public void SetGroupVolume(string pGroupName, float pNormalizedVolume)
        {
            if (string.IsNullOrWhiteSpace(pGroupName) || _AudioBus == null)
                return;

            float lValue = Mathf.Clamp01(pNormalizedVolume);
            float lVolumeDb = Mathf.Log10(Mathf.Max(lValue, 0.0001f)) * 20f;
            _AudioBus.SetFloat($"{pGroupName}Volume", lVolumeDb);
        }

        #endregion
        
        #region _____________________________| DESTROY

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        #endregion

        private class PooledSource
        {
            public AudioSource source;
            public bool inUse;
            public Transform followTarget;

            public PooledSource(AudioSource pSource)
            {
                source = pSource;
                inUse = false;
            }
        }
    }
}