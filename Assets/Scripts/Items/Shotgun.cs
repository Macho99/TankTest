using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun
{
    public override void Attack()
    {
        base.Attack();

    }

    public override bool CanAttack()
    {
        if (!base.CanAttack())
            return false;

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


}
