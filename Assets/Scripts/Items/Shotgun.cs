using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun
{

    public override void Attack()
    {
        isFireTrigger++;
        base.Attack();
        Ray ray = new Ray();
        ray.origin = muzzlePoint.transform.position;
        float distance = 0f;

        ray.direction = (targetPoint - muzzlePoint.transform.position).normalized;
        distance = Vector3.Distance(targetPoint, muzzlePoint.transform.position);


        for (int i = 0; i < ((GunItemSO)itemData).OneShotFireCount; i++)
        {
            float offset = ((GunItemSO)itemData).FireSpread;
            float randOffsetX = Random.Range(-offset, offset);
            float randOffsetY = Random.Range(-offset, offset);

            Vector2 shotOffset = new Vector2(randOffsetX, randOffsetY);
            Vector3 newDirection = (ray.direction + new Vector3(randOffsetX, randOffsetY, 0f)).normalized;

            if (Physics.Raycast(ray.origin, newDirection, out RaycastHit hit, distance + 1f))
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
                {
                    if (hit.collider.TryGetComponent(out IHittable hittable))
                    {
                        hittable.ApplyDamage(owner.transform, hit.point, ray.direction * 2f, ((WeaponItemSO)itemData).Damage);
                    }
                    else
                    {
                        GameObject impact = GameManager.Resource.Instantiate<GameObject>("FX/Particle/DirtImpact", true);
                        impact.transform.position = hit.point;
                        impact.transform.rotation = Quaternion.FromToRotation(impact.transform.forward, hit.normal);

                        Debug.Log("АјАн");

                    }
                }

            }

            Debug.DrawRay(ray.origin, newDirection * distance, Color.blue, 1f);
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
