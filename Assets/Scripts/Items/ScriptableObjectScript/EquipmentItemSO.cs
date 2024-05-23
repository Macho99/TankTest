using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentItemSO : ItemSO
{
    [SerializeField] private EquipmentType equipmentType;
    [SerializeField] private EquipmentSlotType[] slotType;

    public EquipmentType EquipmentType { get { return equipmentType; } }


    public EquipmentSlotType[] SlotTypes { get { return slotType; } }
}

