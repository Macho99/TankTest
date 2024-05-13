using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentItem : Item
{
    [Networked] public PlayerController owner { get; protected set; }

    public virtual void Equip(PlayerController owner)
    {
        this.owner = owner;

    }
    public virtual void UnEquip()
    {
        owner = null;
    }
}
