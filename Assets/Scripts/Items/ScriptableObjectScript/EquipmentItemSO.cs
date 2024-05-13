using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentItemSO : ItemSO
{
<<<<<<< HEAD
    [SerializeField] private EquipmentType equipmentType;
    [SerializeField] private EquipmentSlotType[] slotType;

    public EquipmentType EquipmentType { get { return equipmentType; } }
    public EquipmentSlotType[] SlotType { get { return slotType; } }

=======
    [SerializeField] protected EquipmentSlotType[] slotTypes;
    [SerializeField] protected EquipmentType equipmentType;

    public EquipmentSlotType[] SlotTypes { get { return slotTypes; } }
    public EquipmentType EquipmentType { get { return equipmentType; } }
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
}

public abstract class EquipmentItemInstance : ItemInstance
{
    protected EquipmentItemInstance(ItemSO itemData, int count = 1) : base(itemData, count)
    {

    }
}