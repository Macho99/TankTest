using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentItem : Item
{
    [Networked] protected PlayerController owner { get; set; }
    public virtual void Equip(PlayerController owner)
    {
        this.owner = owner;
        RPC_SetActive(true);
    }
    public virtual void UnEquip()
    {
        owner = null;
        RPC_SetActive(false);
    }

}
