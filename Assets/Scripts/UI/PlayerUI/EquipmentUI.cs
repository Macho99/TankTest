
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EquipmentUI : MonoBehaviour
{

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
        slots[index].SetItem(itemInstance);
    }

    public void OnItemUnEquipment(int index)
    {
        equipment.RPC_Equipment(index, null);
    }

}

