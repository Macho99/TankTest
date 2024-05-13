using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : Weapon
{
    //파츠아이템 
    [SerializeField] protected Transform muzzlePoint;
    [Networked] protected int currentAmmoCount { get; set; }
    protected Transform targetPoint;
    [Networked] protected float currentRefireTime { get; set; }

    public abstract void Attack( Vector3 targetPos);


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
        //  currentAmmoCount = ((GunItemSO)itemInstance.ItemData).MaxAmmoCount;


    }
}
