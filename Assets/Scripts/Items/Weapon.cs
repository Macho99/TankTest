using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : Item
{
    [SerializeField] protected Animator weaponAnim;
    [SerializeField] protected Transform subHandPivot;
    
    public abstract void Attack();

}
