using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSearchUI : MonoBehaviour
{
    [SerializeField] private ItemSearchSlotUI itemSlotUIPrefab;
    [SerializeField] private RectTransform itemSlotRoot;
    private List<ItemSearchSlotUI> itemSlots = new List<ItemSearchSlotUI>();
    private ItemSearchSystem itemSearchSystem;
    private Inventory inventory;
    private int maxCount;
    private void Awake()
    {
        maxCount = 50;
        for (int i = 0; i < maxCount; i++)
        {
            ItemSearchSlotUI itemSlotUI = Instantiate(this.itemSlotUIPrefab, itemSlotRoot.transform).GetComponent<ItemSearchSlotUI>();
            itemSlots.Add(itemSlotUI);
            itemSlotUI.Init(i);
            //itemSlotUI.onItemAcquisition
            itemSlotUI.gameObject.SetActive(false);

        }
    }
    public void Init(ItemSearchSystem itemSearchSystem,Inventory inventory)
    {
        this.itemSearchSystem = itemSearchSystem;
        this.inventory = inventory;
        itemSearchSystem.onUpdate += UpdateSearchItemUI;
    }

    public void UpdateSearchItemUI(int index, ItemInstance itemInstance)
    {
        itemSlots[index].SetItem(itemInstance);
    }

    public void OnItemAcquisition()
    {
        //itemSearchSystem
    }

}
