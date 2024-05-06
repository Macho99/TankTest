using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentItemSO : ItemSO
{


}

public abstract class EquipmentItemInstance : ItemInstance
{
    protected EquipmentItemInstance(ItemSO itemData) : base(itemData)
    {

    }
}