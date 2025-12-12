#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using System.Collections.Generic;
using UnityEngine;

namespace Rush.Game
{
    [CreateAssetMenu(fileName = "SO_Levels", menuName = "Scriptable Objects/Levels")]
    public class SO_LevelCollection : ScriptableObject
    {
        [SerializeField] private List<SO_LevelData> _LevelDatas = new();

        public IReadOnlyList<SO_LevelData> levelDatas => _LevelDatas;
    }
}