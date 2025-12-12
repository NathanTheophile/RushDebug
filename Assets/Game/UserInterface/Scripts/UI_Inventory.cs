#region _____________________________/ INFOS
//  AUTHOR : Nathan THEOPHILE (2025)
//  Engine : Unity
//  Note : MY_CONST, myPublic, m_MyProtected, _MyPrivate, lMyLocal, MyFunc(), pMyParam, onMyEvent, OnMyCallback, MyStruct
#endregion

using System.Collections.Generic;
using Rush.Game;
using UnityEngine;
using static Rush.Game.SO_LevelData;

public class UI_Inventory : MonoBehaviour
{
    #region _____________________________/ REFS

    [Header("References")]
    [SerializeField] private Transform _Container;
    [SerializeField] private UI_Btn_InventoryTile _InventoryTile;

    #endregion

    #region _____________________________/ DATA

    private SO_LevelData _CurrentLevel;

    #endregion

    #region _____________________________| INIT

    private void OnEnable()
    {
        ResetInventory();
    }

    #endregion

    #region _____________________________| METHODS

    public void ResetInventory()
    {
        UI_Btn_InventoryTile.ResetSelection();

        foreach (Transform lChild in _Container)
            Destroy(lChild.gameObject);

        _CurrentLevel = Manager_Game.Instance.CurrentLevel;

        PopulateInventory(Manager_Game.Instance.GetInventoryStatusAtGameStart());
    }

    private void PopulateInventory(IReadOnlyList<InventoryTile> pInventoryOverride)
    {
        if (_CurrentLevel == null) return;

        IReadOnlyList<InventoryTile> lInventorySource = pInventoryOverride ?? _CurrentLevel.inventory;

        foreach (InventoryTile lItem in lInventorySource)
        {
            UI_Btn_InventoryTile lTile = Instantiate(_InventoryTile, _Container);
            lTile.Initialize(lItem);
        }
    }

    #endregion
}