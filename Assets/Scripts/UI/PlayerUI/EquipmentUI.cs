<<<<<<< HEAD
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
=======
using System.Collections;
using System.Collections.Generic;
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
using UnityEngine;

public class EquipmentUI : MonoBehaviour
{
<<<<<<< HEAD
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
=======
    [SerializeField] private ItemSlotUI[] slots;
    private Inventory inventory;
    private Equipment equipment;

    public void Init(Inventory inventory, Equipment equipment)
    {
        this.inventory = inventory;
        this.equipment = equipment;

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Init(ItemSlotType.Static, i);
            slots[i].onItemRightClick += OnItemUnEquipment;

        }
        equipment.onItemUpdate += UpdateItemSlot;
    }
    public void UpdateItemSlot(int index, Item itemInstance)
    {
        Debug.Log(index);
        Debug.Log(itemInstance);
        slots[index].SetItem(itemInstance);
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
    }

    public void OnItemUnEquipment(int index)
    {
<<<<<<< HEAD
        equipment.RPC_UnEquipment(index, null);
    }

}
[Serializable]
public class EquipmentSlotUIData
{
    [SerializeField] public EquipmentSlotType slotType;
    [SerializeField] public ItemSlotUI itemSlot;
}
=======
        equipment.RPC_Equipment(index, null);
    }
}
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
