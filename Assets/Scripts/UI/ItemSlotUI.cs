using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    public enum DurabilityState { Lot, Normal, Few }
    [SerializeField] private TextMeshProUGUI itemCountTMP;
    [SerializeField] private GameObject durabilityObj;
    [SerializeField] private Image durabilityAmountImg;
    [SerializeField] private Sprite[] durabilityAmountImage;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
