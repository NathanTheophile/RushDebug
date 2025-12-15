#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using Rush.Game;
using UnityEngine;

namespace Rush.UI
{
    public class UI_SetMenuCameraOnEnable : MonoBehaviour
    {
        private void OnEnable()
        {
            Manager_Camera.Instance?.SetMenuCameraActive();
        }
    }
}