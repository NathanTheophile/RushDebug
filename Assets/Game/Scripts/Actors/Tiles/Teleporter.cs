#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using UnityEngine;

namespace Rush.Game
{
    public class Teleporter : Tile
    {
        [SerializeField] public Transform pairedTeleporter;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start() => base.Start();
    }
}
