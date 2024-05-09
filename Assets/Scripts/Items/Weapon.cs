using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : EquipmentItem
{
    [SerializeField] protected AnimatorOverrideController weaponAnim;
    [SerializeField] protected Transform subHandPivot;

    public Transform SubHandPivot { get { return subHandPivot; } }

    public abstract void Attack();

}
