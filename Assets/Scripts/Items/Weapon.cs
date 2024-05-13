using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : EquipmentItem
{

    [SerializeField] protected Transform subHandTarget;
    [SerializeField] protected Transform subHandHint;
    protected Transform target;
    public Transform SubHandTarget
    { get { return subHandTarget; } }
    public Transform SubHandHint { get { return subHandHint; } }


    public void SetTarget(Transform subTarget)
    {
        this.target = subTarget;
    }
    public abstract void Attack();

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
    }

    public override void Render()
    {
        if (target != null)
        {
            target.SetPositionAndRotation(subHandTarget.position, subHandTarget.rotation);
        }
    }

  
}

