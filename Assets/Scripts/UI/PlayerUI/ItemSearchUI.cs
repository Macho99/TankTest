using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSearchUI : MonoBehaviour
{
    [SerializeField] private ItemSearchSlotUI itemSlotUIPrefab;
    [SerializeField] private RectTransform itemSlotRoot;
    private ItemSearchSlotUI[] itemSlots;
    private ItemSearchSystem itemSearchSystem;
    private Inventory inventory;

    private int maxCount;

    public void Init(ItemSearchSystem itemSearchSystem, Inventory inventory)
    {
        this.itemSearchSystem = itemSearchSystem;
        this.inventory = inventory;
        maxCount = itemSearchSystem.MaxCount;
        itemSlots = new ItemSearchSlotUI[maxCount];
        for (int i = 0; i < maxCount; i++)
        {
            ItemSearchSlotUI itemSlotUI = Instantiate(this.itemSlotUIPrefab, itemSlotRoot.transform).GetComponent<ItemSearchSlotUI>();
            itemSlots[i] = itemSlotUI;
            itemSlots[i].Init(i);
            itemSlotUI.onItemAcquisition += OnItemAcquisition;
            itemSlotUI.gameObject.SetActive(false);

        }
        itemSearchSystem.onUpdate += UpdateSearchItemUI;
    }

    public void UpdateSearchItemUI(int index, ItemInstance itemInstance)
    {
        itemSlots[index].SetItem(itemInstance);

    }
    public void UpdateSearchItemUI()
    {
      

    }
    public void OnItemAcquisition(int index)
    {
        itemSearchSystem.AcquisitionItem(index);
    }

}
