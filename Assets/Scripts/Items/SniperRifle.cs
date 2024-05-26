using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperRifle : Gun
{
    public override void Attack()
    {
        base.Attack();

        isFireTrigger++;

        Ray ray = new Ray();
        ray.origin = muzzlePoint.transform.position;
        float distance = 0f;

        ray.direction = (targetPoint - muzzlePoint.transform.position).normalized;
        distance = Vector3.Distance(targetPoint, muzzlePoint.transform.position);


        RaycastHit[] results = new RaycastHit[10];
        int resultCount = Physics.RaycastNonAlloc(ray, results, distance + 1f, HitMask);
        if (resultCount > 0)
        {
            for (int i = 0; i < resultCount; i++)
            {
                if (results[i].collider.gameObject.layer != LayerMask.NameToLayer("Player"))
                {
                    IHittable hittable = results[i].collider.GetComponentInParent<IHittable>();
                    if (hittable != null)
                    {
                        hittable.ApplyDamage(owner.transform, results[i].point, ray.direction * 2f, ((WeaponItemSO)itemData).Damage);
                    }
                    else
                    {
                        GameObject impact = GameManager.Resource.Instantiate<GameObject>("FX/Particle/DirtImpact", true);

                        impact.transform.position = results[i].point;
                        impact.transform.rotation = Quaternion.FromToRotation(impact.transform.forward, results[i].normal);
                        break;

                    }
                }
            }
        }

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
