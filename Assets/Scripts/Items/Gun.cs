using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : Weapon
{
    //파츠아이템 
    [SerializeField] protected Transform muzzlePoint;
    protected int currentAmmoCount;
    protected Transform targetPoint;

    public virtual bool CanAttack()
    {
        if (currentAmmoCount <= 0)
            return false;


        return true;
    }
    public virtual void Reload()
    {
        currentAmmoCount = ((GunItemSO)itemInstance.ItemData).MaxAmmoCount;


    }
}
