using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : Weapon
{
    //���������� 
    [SerializeField] protected Transform muzzlePoint;
    protected int currentAmmoCount;
    public virtual void Reload()
    {
        currentAmmoCount = ((GunItemSO)itemData).MaxBulletCount;


    }
}
