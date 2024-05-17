using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bazooka : Gun
{


    public override void Attack()
    {

        base.Attack();

        isFireTrigger++;

        Ray ray = new Ray();
        ray.origin = muzzlePoint.transform.position;
        float distance = 0f;
        if (targetPoint == Vector3.zero)
        {
            ray.direction = muzzlePoint.transform.forward;
            distance = 100f;
        }
        else
        {
            ray.direction = (targetPoint - muzzlePoint.transform.position).normalized;
            distance = Vector3.Distance(targetPoint, muzzlePoint.transform.position);
        }

        if (Physics.Raycast(ray, out RaycastHit hit, distance + 1f))
        {

            // GameManager.Resource.Instantiate();
        }
        Debug.DrawRay(ray.origin, ray.direction * distance, Color.blue);



        targetPoint = Vector3.zero;
        ChangeState(WeaponState.None);
    }


    public override void Reload()
    {

    }


    public override void Equip(PlayerController owner)
    {
        base.Equip(owner);

    }

    public override void UnEquip()
    {

        base.UnEquip();
    }

    public override bool CanAttack()
    {
        if (!base.CanAttack())
            return false;

        return true;
    }

}
