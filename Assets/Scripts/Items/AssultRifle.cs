using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssultRifle : Gun
{



    public override bool CanAttack()
    {
        if (!base.CanAttack())
            return false;

        
        
        return true;
    }
    public override void Attack()
    {
        Ray ray = new Ray();
        ray.origin = muzzlePoint.transform.position;
        ray.direction = (targetPoint.position - muzzlePoint.transform.position).normalized;

        if (Physics.Raycast(ray, out RaycastHit hit, ((GunItemSO)itemData).Distance))
        {
            Debug.Log("HIT");
        }


    }

    public override void Reload()
    {

    }
}
