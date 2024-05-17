using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponState { None, Reload, Put, Draw, Shot }
public abstract class Weapon : EquipmentItem
{

    [SerializeField] protected Transform subHandTarget;
    protected Transform target;
    public Transform SubHandTarget
    { get { return subHandTarget; } }
    [Networked] public WeaponState weaponState { get; protected set; }

    public bool IsTarget { get { return target != null; } }
    public void SetTarget(Transform subTarget)
    {
        this.target = subTarget;
    }
    public abstract void Attack();

    public abstract bool CanAttack();

    public override void FixedUpdateNetwork()
    {

    }

    public override void UnEquip()
    {
        base.UnEquip();
        if (target != null)
        {
            target = null;
        }
        weaponState = WeaponState.None;
    }

    public override void Render()
    {
        if (target != null)
        {
            if (subHandTarget != null)
            {
                target.SetPositionAndRotation(subHandTarget.position, subHandTarget.rotation);

            }
        }
    }
    public void ChangeState(WeaponState state)
    {
        this.weaponState = state;
    }

}

