#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rush.Game
{
    [CreateAssetMenu(fileName = "SO_LevelData", menuName = "Scriptable Objects/LevelData")]
    public class SO_LevelData : ScriptableObject
    {

        [SerializeField] public GameObject levelPrefab;
        [SerializeField] public string levelName;
        [SerializeField] public int stopperTicks;

        [Serializable] public struct InventoryTile
        {
            public Tile.TileVariants type;
            public Tile.TileOrientations orientation;
            public int quantity;
            public Transform tilePrefab;
            public Transform previewPrefab;
        }
        
        public List<InventoryTile> inventory;
    }

}