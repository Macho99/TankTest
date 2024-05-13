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

public abstract class EquipmentItemInstance : ItemInstance
{
    protected EquipmentItemInstance(ItemSO itemData, int count = 1) : base(itemData, count)
    {

    }
}