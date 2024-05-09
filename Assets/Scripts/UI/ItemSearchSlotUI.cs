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

            onItemAcquisition?.Invoke(slotIndex);
        }
    }


}
