using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Fusion.NetworkBehaviour;

public abstract class Gun : Weapon
{
    [Networked, OnChangedRender(nameof(OnFire))] protected int isFireTrigger { get; set; } = 0;
    [Networked] protected Vector3 hitPosition { get; set; }
    [Networked] protected Quaternion hitRotation { get; set; }

    [SerializeField] protected GameObject muzzlePlashPrefab;
    protected NetworkObject muzzleFlashFx;
    //ÆÄÃ÷¾ÆÀÌÅÛ 
    protected AudioSource audioSource;
    [SerializeField] protected Transform muzzlePoint;
    [SerializeField] protected AudioClip fireSFX;
    [Networked] protected Vector3 targetPoint { get; set; }
    [Networked] public int currentAmmoCount { get; protected set; }

    [Networked] public TickTimer refireTimer { get; protected set; }
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public override void Spawned()
    {
        base.Spawned();

    }
    public void SetShotPoint(Vector3 targetPoint)
    {
        this.targetPoint = targetPoint;
    }

    public Vector3 GetMuzzlePoint()
    {
        return muzzlePoint.position;
    }

    public override void Attack()
    {

        refireTimer = TickTimer.CreateFromSeconds(Runner, ((GunItemSO)itemData).FireInterval);
        currentAmmoCount--;
    }
    public override bool CanAttack()
    {
       
        if (currentAmmoCount <= 0)
            return false;

 
        return refireTimer.ExpiredOrNotRunning(Runner);
    }

    public void OnFire()
    {
        if (muzzlePlashPrefab != null)
        {
            GameObject muzzleFlash = GameManager.Resource.Instantiate(muzzlePlashPrefab, muzzlePoint, true);
            muzzleFlash.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        }


    }
    public virtual void Reload(int ammo)
    {
        currentAmmoCount += ammo;

    }
}
