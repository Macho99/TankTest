using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class EquipmentUI : MonoBehaviour
{
    [SerializeField] private EquipmentSlotUIData[] equipItemSlotData;
    private Equipment equipment;
    private Inventory inventory;
    public void Init(Inventory inventory, Equipment equipment)
    {
        this.equipment = equipment;
        this.inventory = inventory;
        for (int i = 0; i < equipItemSlotData.Length; i++)
        {
            if(i < (int)EquipmentSlotType.FirstMainWeapon)
            {
                equipItemSlotData[i].itemSlot.Init((int)equipItemSlotData[i].slotType, ItemSlotUI.ItemIconType.Base, ItemSlotUI.ItemSlotType.Fix);
            }
            else
            {
                equipItemSlotData[i].itemSlot.Init((int)equipItemSlotData[i].slotType, ItemSlotUI.ItemIconType.Detail, ItemSlotUI.ItemSlotType.Fix);

            }

            equipItemSlotData[i].itemSlot.onItemRightClick += OnItemUnEquipment;

        }
        equipment.onItemUpdate += UpdatEquipmentSlot;
    }


    public void UpdatEquipmentSlot(int index, Item itemInstance)
    {
        equipItemSlotData[index].itemSlot.SetItem(itemInstance);
    }

    public void OnItemUnEquipment(int index)
    {
        equipment.RPC_UnEquipment(index, null);
    }

}
[Serializable]
public class EquipmentSlotUIData
{
    [SerializeField] public EquipmentSlotType slotType;
    [SerializeField] public ItemSlotUI itemSlot;
}