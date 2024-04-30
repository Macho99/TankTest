using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssultRifle : Gun
{



    public override void Attack()
    {
        Ray ray = new Ray();
        ray.origin = muzzlePoint.transform.position;
        ray.direction = muzzlePoint.transform.forward;

        if (Physics.Raycast(ray, out RaycastHit hit, ((GunItemSO)itemData).Distance))
        {
            //µ¥¹ÌÁö


        }

    }

    public override void Reload()
    {

    }
}
