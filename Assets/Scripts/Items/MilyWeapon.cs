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
    }

    public override void Equip(PlayerController owner)
    {

    }

    public override void UnEquip()
    {
        base.UnEquip();
    }
}
