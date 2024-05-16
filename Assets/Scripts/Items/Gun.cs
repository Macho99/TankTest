using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Fusion.NetworkBehaviour;

public abstract class Gun : Weapon
{
    [Networked, OnChangedRender(nameof(OnFire))] protected bool isFireTrigger { get; set; }
    [SerializeField] protected NetworkPrefabRef muzzlePlashPrefab;
    protected NetworkObject muzzleFlashFx;
    //파츠아이템 
    protected AudioSource audioSource;
    [SerializeField] protected Transform muzzlePoint;
    [SerializeField] protected AudioClip fireSFX;
    [Networked] protected Vector3 targetPoint { get; set; }
    [Networked] public int currentAmmoCount { get; private set; }

    [Networked] protected TickTimer refireTimer { get; set; }
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


    public override void Attack()
    {
        if (muzzleFlashFx != null)
        {
            Runner.Despawn(muzzleFlashFx);
            muzzleFlashFx = null;
            Debug.Log("디스폰");
        }
        audioSource.clip = fireSFX;
        audioSource.Play();

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

    public void OnFire()
    {
        
        Debug.Log("Play");
    }
    public virtual void Reload()
    {



    }
}
