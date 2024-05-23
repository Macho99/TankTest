using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bazooka : Gun
{
    [SerializeField] private GameObject warhead;


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

        bool isHit = Physics.Raycast(ray, out RaycastHit hit, distance + 1f);
        if (isHit == false)
        {
            hit.point = ray.origin + ray.direction * distance;
        }
        if (HasStateAuthority)
        {
            AttackRPGRocket rpgRocketPrefab = GameManager.Resource.Load<AttackRPGRocket>("Item/Others/RPG_Warhead");
            AttackRPGRocket rpgRocket = Runner.Spawn(rpgRocketPrefab, transform.position, transform.rotation);
            rpgRocket.Init(hit.point, ((WeaponItemSO)itemData).Damage);
            Debug.Log("╬Нец");
        }

        warhead.gameObject.SetActive(false);

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

    public override void Reload(int ammo)
    {
        base.Reload(ammo);
        warhead.gameObject.SetActive(true);
    }
    public override bool CanAttack()
    {
        if (!base.CanAttack())
            return false;

        return true;
    }

}
