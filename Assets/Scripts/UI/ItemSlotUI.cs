using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    public enum ItemSlotType { Inventory, Equipment }
    public enum DurabilityState { Lot, Normal, Few }

    [SerializeField] private Sprite itemEmptyIcon;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI itemCountTMP;
    [SerializeField] private GameObject durabilityObj;
    [SerializeField] private Image durabilityAmountImg;
    [SerializeField] private Sprite[] durabilityAmountImage;


    private bool isEmpty;

    public ItemSlotType itemSlotType { get; private set; }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetItem(Item item)
    {
        isEmpty = item == null ? true : false;
        CheckEmpty();



      


    }
    private void CheckEmpty()
    {
        durabilityObj.SetActive(!isEmpty);
        itemCountTMP.enabled = !isEmpty;
        itemIconImage.sprite = isEmpty ? itemEmptyIcon : itemIconImage.sprite;
    }
}
