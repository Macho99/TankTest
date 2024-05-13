using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSearchUI : MonoBehaviour
{
    [SerializeField] private ItemSlotUI itemSlotUIPrefab;
    [SerializeField] private RectTransform itemSlotRoot;
    private ItemSlotUI[] itemSlots;
    private ItemSearchSystem itemSearchSystem;
    private Inventory inventory;

    private int maxCount;

    public void Init(ItemSearchSystem itemSearchSystem, Inventory inventory)
    {
        this.itemSearchSystem = itemSearchSystem;
        this.inventory = inventory;
        maxCount = itemSearchSystem.MaxCount;
        itemSlots = new ItemSlotUI[maxCount];
        for (int i = 0; i < maxCount; i++)
        {
            ItemSlotUI itemSlotUI = Instantiate(this.itemSlotUIPrefab, itemSlotRoot.transform);
            itemSlots[i] = itemSlotUI;
            itemSlots[i].Init(ItemSlotType.Dynamic,i);
            itemSlotUI.onItemRightClick += OnItemAcquisition;
            itemSlotUI.gameObject.SetActive(false);

        }
        itemSearchSystem.onUpdate += UpdateSearchItemUI;
    }

    public void UpdateSearchItemUI(int index, Item itemInstance)
    {
        itemSlots[index].SetItem(itemInstance);

    }

    public void OnItemAcquisition(int index)
    {
        itemSearchSystem.AcquisitionItem(index);
    }

}
