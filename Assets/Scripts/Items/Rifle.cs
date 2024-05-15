using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rifle : Gun
{


    public override void Attack()
    {
        IsFire = true;
        //muzzlePlashFX.Play();
        Ray ray = new Ray();
        ray.origin = muzzlePoint.transform.position;
        ray.direction = muzzlePoint.forward.normalized;

        if (Physics.Raycast(ray.origin, ray.direction, 50))
        {
            Debug.Log("╬Нец");
        }
        Debug.DrawRay(ray.origin, ray.direction * 50, Color.red);
        Debug.Log("attack");
        currentRefireTime = ((GunItemSO)itemData).FireInterval;
        StartCoroutine(RefireRoutine());

    }


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

    public override bool CanAttack()
    {
        if (!base.CanAttack())
            return false;

        return true;
    }

    protected override void OnFire()
    {

        muzzlePlashFX.Play();
        IsFire = !IsFire;


        Debug.Log(IsFire);
    }
}
