using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

<<<<<<< HEAD

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

=======
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
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
    protected bool isEmpty;
    public event Action<int> onItemRightClick;


<<<<<<< HEAD
    public void Init(int slotIndex, ItemIconType itemIconType, ItemSlotType itemSlotType)
    {
        this.iconType = itemIconType;
=======
    public void Init(ItemSlotType slotType, int slotIndex)
    {
        this.slotType = slotType;
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
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
<<<<<<< HEAD
        if (iconType == ItemIconType.Detail)
        {
            itemIconImage.sprite = ((WeaponItemSO)item.ItemData).GetItemDetailIcon();
        }
        else
        {
            itemIconImage.sprite = item.ItemData.ItemIcon;
        }
=======
        if (slotIconType == ItemSlotIconType.Base)
            itemIconImage.sprite = item.ItemData.ItemIcon;
        else
            itemIconImage.sprite = ((WeaponItemSO)item.ItemData).ItemDetailIcon;
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4

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
<<<<<<< HEAD
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






=======
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
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
    }

    public void OnPointerClick(PointerEventData eventData)
    {
<<<<<<< HEAD

        if (eventData.button == PointerEventData.InputButton.Right)
        {

=======
        if (eventData.button == PointerEventData.InputButton.Right)
        {
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
            if (isEmpty == true)
                return;

            onItemRightClick?.Invoke(slotIndex);
        }
<<<<<<< HEAD

=======
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
    }


}
