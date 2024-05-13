using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemContainerUI : PopUpUI
{
    [SerializeField] private ItemSearchUI searchUI;
    [SerializeField] private InventoryUI invenUI;
    [SerializeField] private EquipmentUI equipmentUI;
<<<<<<< HEAD
=======

>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
    private ItemContainer itemContainer;
    public void Init(ItemContainer itemContainer)
    {
        this.itemContainer = itemContainer;
        searchUI.Init(itemContainer.itemSearchSystem, itemContainer.inventory);
        invenUI.Init(itemContainer.inventory, itemContainer.equipment);
        equipmentUI.Init(itemContainer.inventory, itemContainer.equipment);
<<<<<<< HEAD
=======

>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
    }

}
