using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : Weapon
{
    [SerializeField] protected NetworkPrefabRef muzzlePlashPrefab;
    protected NetworkObject muzzleFlashFx;
    //���������� 
    [SerializeField] protected Transform muzzlePoint;
    [Networked] protected Vector3 targetPoint { get; set; }
    [Networked] protected int currentAmmoCount { get; set; }

    [Networked] protected TickTimer refireTimer { get; set; }
    public override void Spawned()
    {
        base.Spawned();
    }
    public void SetShotPoint(Vector3 targetPoint)
    {
        this.targetPoint = targetPoint;
    }

    public override void Attack()
    {
        if (muzzleFlashFx != null)
        {
            Runner.Despawn(muzzleFlashFx);
            muzzleFlashFx = null;
            Debug.Log("����");
        }

        if (HasStateAuthority)
            muzzleFlashFx = Runner.Spawn(muzzlePlashPrefab, muzzlePoint.position, muzzlePoint.rotation);


    }
    public override bool CanAttack()
    {
        if (!refireTimer.ExpiredOrNotRunning(Runner))
            return false;


        refireTimer = TickTimer.CreateFromSeconds(Runner, ((GunItemSO)itemData).FireInterval);

        return true;
    }


    public virtual void Reload()
    {



    }
}
