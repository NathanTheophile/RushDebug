#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using System.Collections.Generic;
using DG.Tweening;
using Rush.Game.Core;
using Unity.VisualScripting;
using UnityEngine;
using static Rush.Game.SO_Colors;

namespace Rush.Game
{
    public class Spawner : Tile
    {
        [SerializeField] private Cube cubePrefab;
        
        [SerializeField] private SO_Colors _ColorSO;   
                [SerializeField] GameObject tomb;
                                [SerializeField] GameObject bone;
bool _SkipAnimation = false;
[SerializeField] Material _CubeMaterial;
        Manager_Time timeManager;
        Manager_Tile tileManager;
        Manager_Game gameManager;
        private Color _Color;
        [SerializeField] private ColorsData _ColorData;
        private Material _Material;
        private Material _Emissive;
        [SerializeField] private int _StartDelay = 0;

        [SerializeField] private int _TickBetweenSpawns = 2;
        private int _CurrentWaitStatus = 2;
        [SerializeField] private int _AmountoOfCubes = 1;
        private int _CurrentCubeSpawned = 0;
        private bool _Spawning = false;
        bool _DeadCube = true;
        public List<Cube> _SpawnerBabies = new List<Cube>();

        [Header("Tween")]
        [SerializeField] private float _SpawnTweenDuration = 0.35f;
        [SerializeField] private Vector3 _SpawnTweenRotation = new Vector3(0f, 360f, 0f);


        void Awake()
        {
            _Material = _ColorSO.Material;
            _Emissive = _ColorSO.Emissive;
            _Color = _ColorSO.Color;
            _ColorData = _ColorSO.ColorData;
            _CubeMaterial = _ColorSO.CubeMaterial;
            var tombR = tomb.GetComponent<Renderer>();
            var tombM = tombR.materials;
            tombM[0] = _Material;
            tombM[1] = _Emissive;
            tombR.materials = tombM;
            bone.GetComponent<Renderer>().material = _Emissive;
        }

        protected override void Start()
        {
            base.Start();
            timeManager = Manager_Time.Instance;
            tileManager = Manager_Tile.Instance;
            gameManager = Manager_Game.Instance;
            _CurrentWaitStatus = _TickBetweenSpawns;
            gameManager.onGameRetry += ResetSpawner;
            gameManager.onGameStart += StartGame;
            gameManager?.UpdateCubesAmountoComplete(_AmountoOfCubes);
            timeManager.onTickFinished += Countdown;
        }

        void ResetSpawner()
        {
            _Spawning = false;

            _CurrentWaitStatus = _StartDelay;
            _CurrentCubeSpawned = 0;
            Debug.Log("BeforeDestroycub. " + _SpawnerBabies.Count);
            _DeadCube = false;
            for (int i = _SpawnerBabies.Count - 1; i >= 0; i--)
            {
                Debug.Log("During. " + i);

                HandleCubeValidation(_SpawnerBabies[i]);
            }
            _SpawnerBabies.Clear();

            _DeadCube = true;
            if (bone != null) bone.SetActive(true);
                        timeManager.onTickFinished += Countdown;

        }

        void SpawnCube()
        {
            Cube lCube = Instantiate(cubePrefab, transform.position, Quaternion.identity);
            PlaySpawnTween(lCube.transform);
            lCube.SetColor(_CubeMaterial);
            timeManager.objectsAffectedByTime.Add(lCube);
            timeManager.onTickFinished += lCube.TickUpdate;
            lCube.onTileDetected += tileManager.TryGetTile;
            lCube.onCubeDeath += HandleCubeValidation;
            lCube.onCubeDeath += gameManager.GameOver;
            lCube.ColorData = _ColorData;
            lCube.SpawnDirection(direction);
            _CurrentCubeSpawned++;
            _SpawnerBabies.Add(lCube);
            if (_CurrentCubeSpawned >= _AmountoOfCubes) StopSpawning();
        }

        void StartGame()
        {
            if (bone != null) bone.SetActive(false);
            _Spawning = true;
        }

        private void Countdown(int pTick)
        {
            if (_StartDelay > 0) {_StartDelay--; return;}

            _CurrentWaitStatus --;
            if (_CurrentWaitStatus <= 0)
            {
                if (_Spawning) SpawnCube();

                _CurrentWaitStatus = _TickBetweenSpawns;

            }
        }

        private void StopSpawning()
        {
                        if (timeManager == null) return;
            timeManager.onTickFinished -= Countdown;
        }

        private void PlaySpawnTween(Transform pCubeTransform)
        {
            if (pCubeTransform == null) return;

            float lDuration = _SpawnTweenDuration / Mathf.Max(timeManager.GlobalTickSpeed, Mathf.Epsilon);
            Vector3 lSpawnPosition = transform.position;
            Vector3 lTargetScale = pCubeTransform.localScale;

            pCubeTransform.localScale = Vector3.zero;
            pCubeTransform.position = lSpawnPosition + Vector3.down;

            DG.Tweening.Sequence lSequence = DOTween.Sequence();
            lSequence.Join(pCubeTransform
                .DOMove(lSpawnPosition, lDuration)
                .SetEase(Ease.OutBack));

            lSequence.Join(pCubeTransform
                .DOScale(lTargetScale, lDuration)
                .SetEase(Ease.OutBounce));

            lSequence.Join(pCubeTransform
                .DORotate(pCubeTransform.eulerAngles + _SpawnTweenRotation, lDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuint));
        }

        private void HandleCubeValidation(Cube pCube)
        {
            Debug.Log("HandleCubeVal");
            pCube.SetModePause();
            timeManager.onTickFinished -= pCube.TickUpdate;
            timeManager.objectsAffectedByTime.Remove(pCube);
            pCube.onTileDetected -= tileManager.TryGetTile;
            pCube.onCubeDeath -= gameManager.GameOver;
            pCube.onCubeDeath -= HandleCubeValidation;

            if (_DeadCube)  _SpawnerBabies.Remove(pCube);

            pCube.PlayValidationTween(() => Destroy(pCube.GameObject()));
        }

        private void OnDestroy()
        {
            StopSpawning();
            if (gameManager != null)
            {
                gameManager.onGameRetry -= ResetSpawner;
                gameManager.onGameStart -= StartGame;
            }

            if (timeManager == null) return;

for (int i = _SpawnerBabies.Count - 1; i >= 0; i--)
            {
                _SkipAnimation = true;
                HandleCubeValidation(_SpawnerBabies[i]);
            }
        }
    }
}