using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public enum ItemSlotType { Static, Dynamic }
public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{

    public enum ItemSlotIconType { Base, Detail }
    protected int slotIndex;
    private ItemSlotType slotType;
    [SerializeField] private ItemSlotIconType slotIconType;
    [SerializeField] protected TextMeshProUGUI itemNameTMP;
    [SerializeField] protected Image itemIconImage;
    [SerializeField] protected TextMeshProUGUI itemCountTMP;
    [SerializeField] protected Image durabilityAmountImg;
    [SerializeField] private TextMeshProUGUI ammoNameTMP;
    [SerializeField] private TextMeshProUGUI ammoNameCount;
    protected bool isEmpty;
    public event Action<int> onItemRightClick;



    public void Init(ItemSlotType slotType, int slotIndex)
    {
        this.slotType = slotType;
        this.slotIndex = slotIndex;
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

        if (slotIconType == ItemSlotIconType.Base)
            itemIconImage.sprite = item.ItemData.ItemIcon;
        else
            itemIconImage.sprite = ((WeaponItemSO)item.ItemData).ItemDetailIcon;

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

        if (itemCountTMP != null)
        {
            itemCountTMP.enabled = !isEmpty;
            itemCountTMP.enabled = false;
            itemCountTMP.text = string.Empty;
        }

        itemIconImage.enabled = isEmpty ? false : true;
        if (itemNameTMP != null)
        {
            itemNameTMP.enabled = !isEmpty;
        }
        if (ItemSlotType.Dynamic == slotType)
            gameObject.SetActive(!isEmpty);
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
