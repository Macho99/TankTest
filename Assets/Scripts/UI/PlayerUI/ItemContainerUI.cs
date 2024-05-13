using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemContainerUI : PopUpUI
{
    [SerializeField] private ItemSearchUI searchUI;
    [SerializeField] private InventoryUI invenUI;
    [SerializeField] private EquipmentUI equipmentUI;

    private ItemContainer itemContainer;
    public void Init(ItemContainer itemContainer)
    {
        this.itemContainer = itemContainer;
        searchUI.Init(itemContainer.itemSearchSystem, itemContainer.inventory);
        invenUI.Init(itemContainer.inventory, itemContainer.equipment);
        equipmentUI.Init(itemContainer.inventory, itemContainer.equipment);
    }

}
