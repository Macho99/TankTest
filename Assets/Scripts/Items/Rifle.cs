using Fusion;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Rifle : Gun
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

        Debug.DrawRay(ray.origin, ray.direction * distance, Color.blue);

      

        if (Physics.Raycast(ray, out RaycastHit hit, distance + 1f, HitMask))
        {
            if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
            {
                IHittable hittable = hit.collider.GetComponentInParent<IHittable>();

				if (hittable != null)
                {
                    hittable.ApplyDamage(owner.transform, hit.point, ray.direction * 2f, ((WeaponItemSO)itemData).Damage);
                }
                else
                {
                    GameObject impact = GameManager.Resource.Instantiate<GameObject>("FX/Particle/DirtImpact", true);

                    impact.transform.position = hit.point;
                    impact.transform.rotation = Quaternion.FromToRotation(impact.transform.forward, hit.normal);

                }
            }

        }
        print(currentAmmoCount);
       
        targetPoint = Vector3.zero;
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
