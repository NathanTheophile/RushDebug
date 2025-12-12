#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using System.Collections.Generic;
using Rush.Game;
using UnityEngine;

namespace Rush.UI
{
    public class UI_Grid_LevelSelector : MonoBehaviour
    {
        #region _____________________________/ REFS

        [Header("References")]
        [SerializeField] private SO_LevelCollection _LevelCollection;
        [SerializeField] private UI_Btn_Level _LevelItemPrefab;

        #endregion

        #region _____________________________/ PREVIEW

        [Header("Preview")]
        [SerializeField] private float _PreviewOffset = 50f;
        [SerializeField] private Vector3 _PreviewOrigin = new Vector3(10000f, 10000f, 10000f);

        #endregion

        #region _____________________________/ MISC

        private readonly List<UI_Btn_Level> _SpawnedLevelInstances = new();

        #endregion

        #region _____________________________| UNITY

        private void OnEnable() { TilePlacer.Instance?.ResetPlacedTiles(); Populate();}

        private void OnDisable()
        {
            foreach (var level in _SpawnedLevelInstances)
            {
                Destroy(level.instantiatedLevel);
                Destroy(level.gameObject);
            }

            _SpawnedLevelInstances.Clear();
        }

        private void OnDestroy()
        {
            foreach (var instance in _SpawnedLevelInstances)
            {
                Destroy(instance);
            }

            _SpawnedLevelInstances.Clear();
        }

        #endregion

        #region _____________________________| METHODS

        private void Populate()
        {
            var lLevelCollection = _LevelCollection.levelDatas;

            for (int i = 0; i < lLevelCollection.Count; i++)
            {
                Vector3 lSpawnPosition = _PreviewOrigin + new Vector3(_PreviewOffset * i, 0f, 0f);

                UI_Btn_Level levelItem = Instantiate(_LevelItemPrefab, transform, false);
                levelItem.Initialize(lSpawnPosition, lLevelCollection[i], transform.parent);

                _SpawnedLevelInstances.Add(levelItem);
            }
        }

        #endregion
    }
}