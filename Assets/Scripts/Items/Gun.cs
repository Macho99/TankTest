using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : Weapon
{
    [SerializeField] protected ParticleSystem muzzlePlashFX;
    //ÆÄÃ÷¾ÆÀÌÅÛ 
    [SerializeField] protected Transform muzzlePoint;
    [Networked] protected int currentAmmoCount { get; set; }
    protected Transform targetPoint;
    [Networked] protected float currentRefireTime { get; set; }
    [Networked, OnChangedRender(nameof(OnFire))] protected NetworkBool IsFire { get; set; }
    public override void Spawned()
    {
        if (HasStateAuthority)
        {
             IsFire = false;
            currentRefireTime = 0f;
        }
        base.Spawned();
    }
    protected abstract void OnFire();
    public abstract void Attack(Vector3 targetPos);

    public override bool CanAttack()
    {
     
        //if (currentAmmoCount <= 0)
        //    return false;

        if (currentRefireTime > 0f)
            return false;

        return true;
    }

    public IEnumerator RefireRoutine()
    {
        while (0f < currentRefireTime)
        {
            currentRefireTime -= Time.deltaTime;
            yield return null;
        }
        currentRefireTime = 0f;
    }

    public virtual void Reload()
    {



    }
}
