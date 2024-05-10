using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryItemSlotUI itemSlotUIPrefab;
    [SerializeField] private RectTransform slotRoot;
    private InventoryItemSlotUI[] slots;
    private Inventory inventory;
    private Equipment equipment;

    public void Init(Inventory inventory, Equipment equipment)
    {
        this.inventory = inventory;
        this.equipment = equipment;
        slots = new InventoryItemSlotUI[inventory.MaxCount];
        for (int i = 0; i < slots.Length; i++)
        {
            InventoryItemSlotUI itemSlotUI = Instantiate(this.itemSlotUIPrefab, slotRoot.transform);
            slots[i] = itemSlotUI;
            slots[i].Init(i);
            itemSlotUI.onItemEquipment += OnItemEquipment;
            itemSlotUI.gameObject.SetActive(false);

        }
        inventory.onItemUpdate += UpdateInventorySlot;
    }
    public void UpdateInventorySlot(int index, Item itemInstance)
    {
        Debug.Log(slots.Length);
        Debug.Log(index);
        slots[index].SetItem(itemInstance);
    }

    public void OnItemEquipment(int index)
    {
        Item item = inventory.GetItem(index);
        equipment.RPC_Equipment(index, item);
    }



}
