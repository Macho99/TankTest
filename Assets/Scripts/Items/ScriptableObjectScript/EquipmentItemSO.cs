using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentItemSO : ItemSO
{
    [SerializeField] protected EquipmentSlotType[] slotTypes;
    [SerializeField] protected EquipmentType equipmentType;

    public EquipmentSlotType[] SlotTypes { get { return slotTypes; } }
    public EquipmentType EquipmentType { get { return equipmentType; } }
}

public abstract class EquipmentItemInstance : ItemInstance
{
    protected EquipmentItemInstance(ItemSO itemData, int count = 1) : base(itemData, count)
    {

    }
}