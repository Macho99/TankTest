using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : Gun
{

    public override void Attack()
    {
        base.Attack();

        isFireTrigger = !isFireTrigger;
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

        if (Physics.Raycast(ray, out RaycastHit hit, distance))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Monster"))
            {
                if (hit.collider.TryGetComponent(out IHittable hittable))
                {
                    hittable.ApplyDamage(owner.transform, hit.point, ray.direction * 2f, ((WeaponItemSO)itemData).Damage);
                }
            }
            else
            {
                GameObject imaptPrefab = GameManager.Resource.Load<GameObject>("FX/Particle/DirtImpact");

                GameObject impact = GameManager.Pool.Get(imaptPrefab);

                impact.transform.position = hit.point;
                impact.transform.rotation = Quaternion.FromToRotation(impact.transform.forward, hit.normal);


            }
        }

        Debug.DrawRay(ray.origin, ray.direction * distance, Color.blue);

        Debug.Log("attack");

        targetPoint = Vector3.zero;
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
