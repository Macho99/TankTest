using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revolver : Gun
{
    public override void Attack()
    {
        base.Attack();
        Debug.Log("attack");

        isFireTrigger++;

        Ray ray = new Ray();
        ray.origin = muzzlePoint.transform.position;

        ray.direction = (targetPoint - muzzlePoint.transform.position).normalized;
        float distance = Vector3.Distance(targetPoint, muzzlePoint.transform.position);


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


        targetPoint = Vector3.zero;


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
