using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    protected int slotIndex;
    [SerializeField] protected TextMeshProUGUI itemNameTMP;
    [SerializeField] protected Image itemIconImage;
    [SerializeField] protected TextMeshProUGUI itemCountTMP;
    [SerializeField] protected Image durabilityAmountImg;

    protected bool isEmpty;
    public event Action<int> onItemRightClick;

  
    public void Init(int slotIndex)
    {
        this.slotIndex = slotIndex;
    }
    public void SetItem(ItemInstance item)
    {
        isEmpty = item == null ? true : false;
        CheckEmpty();
        if (isEmpty)
            return;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        itemIconImage.enabled = true;
        itemIconImage.sprite = item.ItemData.ItemIcon;
        itemNameTMP.text = item.ItemData.ItemName;

    }
    private void CheckEmpty()
    {
        itemCountTMP.enabled = !isEmpty;
        itemIconImage.enabled = isEmpty ? false : true;
        onItemRightClick = null;
        gameObject.SetActive(false);
    }

    public abstract void OnPointerClick(PointerEventData eventData);

    public void OnPointerRightClickEvent()
    {
        onItemRightClick?.Invoke(slotIndex);
    }
}
