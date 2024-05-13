using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MilyWeapon : Weapon
{
    public override void Attack()
    {
        
    }

    public override bool CanAttack()
    {
        return true;
        //throw new System.NotImplementedException();
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
