using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentItem : Item
{
<<<<<<< HEAD
    [Networked] protected PlayerController owner { get; set; }
    public virtual void Equip(PlayerController owner)
    {
        this.owner = owner;
        RPC_SetActive(true);
=======
    [Networked] public PlayerController owner { get; protected set; }

    public virtual void Equip(PlayerController owner)
    {
        this.owner = owner;

>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
    }
    public virtual void UnEquip()
    {
        owner = null;
<<<<<<< HEAD
        RPC_SetActive(false);
    }

=======
    }
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
}
