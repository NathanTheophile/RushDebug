#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using UnityEngine;

namespace Rush.Game
{
    public class Dispatcher : Tile
    {
        private bool _Switcher = true;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        protected override void Start() 
        {
            m_Direction = Vector3.right;
            Manager_Game.Instance.onGameRetry += ResetDispatcher;
        }

        void ResetDispatcher()
        {
            m_Direction = Vector3.right;
        }


        public void Switch()
        {
            if (_Switcher) { m_Direction = Vector3.left; _Switcher = false; }
            else { m_Direction = Vector3.right; _Switcher = true; }
        }
    }
}
