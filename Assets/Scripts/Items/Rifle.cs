using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : Gun
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
        ray.direction = muzzlePoint.forward.normalized;

<<<<<<< HEAD
        Debug.DrawRay(ray.origin, ray.direction * 50, Color.red);
        Debug.Log("attack");
        currentRefireTime = ((GunItemSO)itemData).FireInterval;
        StartCoroutine(RefireRoutine());
    }
    public override void Render()
    {
        if (transform.parent != null)
        {
            //Debug.Log(transform.parent.name);
        }
    }
=======
    }  
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4

    public override void Reload()
    {

    }
    public override void Attack(Vector3 targetPoint)
    {



        Ray ray = new Ray();
        ray.origin = muzzlePoint.transform.position;
        Vector3 distance = targetPoint - muzzlePoint.transform.position;
        ray.direction = distance.normalized;

        Debug.DrawRay(ray.origin, distance, Color.red);
        Debug.Log("attack");
        StartCoroutine(RefireRoutine());
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
