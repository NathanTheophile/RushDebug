#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using UnityEngine;
using UnityEngine.UI;

namespace Rush.UI
{
    [RequireComponent(typeof(Button))]
    public class UI_Btn_ResetTiles : MonoBehaviour
    {
        #region _____________________________/ REFS
    
        [Header("References")]
        [SerializeField] private Button _Button;
        [SerializeField] private UI_Inventory _Inventory;
    
        #endregion
    
        #region _____________________________| UNITY
        
        private void Init() { if (_Button == null) _Button = GetComponent<Button>(); }
    
        void Start()
        {
            Init();
    
            if (_Button == null) return;
    
            _Button.onClick.AddListener(ResetTiles);
        }
    
        #endregion
    
        #region _____________________________| UI METHODS
        private void ResetTiles()
        {
            TilePlacer.Instance?.ResetPlacedTiles();
            _Inventory?.ResetInventory();
        }
    
        #endregion
    }
}
