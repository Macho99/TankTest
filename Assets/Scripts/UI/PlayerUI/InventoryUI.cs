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

    public void Init(Inventory inventory)
    {
        this.inventory = inventory;

        slots = new InventoryItemSlotUI[inventory.MaxCount];
        Debug.Log(slots.Length);
        for (int i = 0; i < slots.Length; i++)
        {
            InventoryItemSlotUI itemSlotUI = Instantiate(this.itemSlotUIPrefab, slotRoot.transform);
            slots[i] = itemSlotUI;
            slots[i].Init(i);
            //itemSlotUI.onItemEquipment += OnItemAcquisition;
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
        // inventory.AcquisitionItem(index);
    }
}
