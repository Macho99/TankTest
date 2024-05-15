using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun
{
    public override void Attack()
    {
        //throw new System.NotImplementedException();
    }

    public override void Attack(Vector3 targetPos)
    {
        //throw new System.NotImplementedException();
    }

    public override bool CanAttack()
    {
        return true;
    }

    public override void Equip(PlayerController owner)
    {
        base.Equip(owner);

    }

    public override void UnEquip()
    {
        base.UnEquip();
    }

    protected override void OnFire()
    {
        //throw new System.NotImplementedException();
    }
}
