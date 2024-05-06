using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSearchUI : MonoBehaviour
{
    [SerializeField] private ItemSearchSlotUI itemSlotUIPrefab;
    [SerializeField] private RectTransform itemSlotRoot;
    private List<ItemSearchSlotUI> itemSlots = new List<ItemSearchSlotUI>();
    private ItemSearchSystem itemSearchSystem;
    private int maxCount;
    private void Awake()
    {
        maxCount = 50;
        for (int i = 0; i < maxCount; i++)
        {
            ItemSearchSlotUI itemSlotUI = Instantiate(this.itemSlotUIPrefab, itemSlotRoot.transform).GetComponent<ItemSearchSlotUI>();
            itemSlots.Add(itemSlotUI);
            itemSlotUI.Init(i);
            itemSlotUI.gameObject.SetActive(false);

        }
    }
    public void Init(ItemSearchSystem itemSearchSystem)
    {
        this.itemSearchSystem = itemSearchSystem;
        itemSearchSystem.onUpdate += UpdateSearchItemUI;
    }
    private void Start()
    {
     
        Debug.Log(itemSlots.Count);
    }
  
    public void UpdateSearchItemUI(int index, ItemInstance itemInstance)
    {
        Debug.Log(itemSlots.Count);
        Debug.Log(index);
        itemSlots[index].SetItem(itemInstance);
    }


}
