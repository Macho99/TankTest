using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSearchSlotUI : ItemSlotUI
{
    public event Action<int> onItemAcquisition;
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (isEmpty == true)
                return;

            Debug.Log(itemNameTMP.text);
            Debug.Log(slotIndex);
            onItemAcquisition?.Invoke(slotIndex);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
