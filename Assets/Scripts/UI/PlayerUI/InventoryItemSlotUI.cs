using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItemSlotUI : ItemSlotUI
{
    public event Action<int> onItemEquipment;
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (isEmpty == true)
                return;

            onItemEquipment?.Invoke(slotIndex);
        }
    }



}
