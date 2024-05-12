using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    public enum ItemIconType { Base, Detail }
    public enum ItemSlotType { Creative, Fix }
    protected int slotIndex;
    private ItemIconType iconType;
    private ItemSlotType slotType;
    [SerializeField] private TextMeshProUGUI itemNameTMP;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemCountTMP;
    [SerializeField] private Image durabilityAmountImg;
    [SerializeField] private TextMeshProUGUI ammoNameTMP;
    [SerializeField] private TextMeshProUGUI ammoCountTMP;

    protected bool isEmpty;
    public event Action<int> onItemRightClick;


    public void Init(int slotIndex, ItemIconType itemIconType, ItemSlotType itemSlotType)
    {
        this.iconType = itemIconType;
        this.slotIndex = slotIndex;
        this.slotType = itemSlotType;
    }
    public virtual void SetItem(Item item)
    {
        isEmpty = item == null ? true : false;
        CheckEmpty();
        if (isEmpty)
            return;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);


        itemIconImage.enabled = true;
        if (iconType == ItemIconType.Detail)
        {
            itemIconImage.sprite = ((WeaponItemSO)item.ItemData).GetItemDetailIcon();
        }
        else
        {
            itemIconImage.sprite = item.ItemData.ItemIcon;
        }

        if (itemNameTMP != null)
        {
            itemNameTMP.enabled = true;
            itemNameTMP.text = item.ItemData.ItemName;
        }

        if (item.ItemData.IsStackable)
        {
            if (itemCountTMP != null)
            {
                itemCountTMP.enabled = true;
                itemCountTMP.text = item.currentCount.ToString();
            }
        }

    }

    private void CheckEmpty()
    {
        if (isEmpty)
        {
            if (itemCountTMP != null)
            {
                itemCountTMP.enabled = false;
                itemCountTMP.text = string.Empty;
                itemCountTMP.enabled = false;
            }
            if (itemNameTMP != null)
            {
                itemNameTMP.text = string.Empty;
                itemNameTMP.enabled = false;
            }
            itemIconImage.enabled = false;

            if (slotType == ItemSlotType.Creative)
                gameObject.SetActive(false);
        }






    }

    public void OnPointerClick(PointerEventData eventData)
    {

        if (eventData.button == PointerEventData.InputButton.Right)
        {

            if (isEmpty == true)
                return;

            onItemRightClick?.Invoke(slotIndex);
        }

    }


}
