using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : EquipmentItem
{
    [SerializeField] protected Transform subHandTarget;
    [SerializeField] protected Transform subHandHint;
    public Transform SubHandTarget { get { return subHandTarget; } }
    public Transform SubHandHint { get { return subHandHint; } }

    public abstract void Attack();

}
