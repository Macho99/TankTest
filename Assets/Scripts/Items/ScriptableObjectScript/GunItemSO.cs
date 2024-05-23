using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum FireBehaviorType { Single, Multi }

[CreateAssetMenu(fileName = "New Gun", menuName = "SO/ItemSO/Weapon/Gun")]

public class GunItemSO : WeaponItemSO
{

    [SerializeField, Range(0, 1000f)] private float fireSpeed;
    [SerializeField, Range(0, 5f)] private float fireInterval;
    [SerializeField, Range(0, 500f)] private float distance;
    [SerializeField, Range(0, 50)] private int maxAmmoCount;
    [SerializeField, Range(0f, 10f)] private float reloadTime;
    [SerializeField, Range(0f, 10f)] private float ReboundHealthSpeed;
    [SerializeField] private AmmoType ammoType;
    [SerializeField] private FireBehaviorType fireBehaviorType;
    [SerializeField, Range(0, 20)] private int oneShotFireCount = 1;
    [SerializeField, Range(0f, 50f)] private float fireSpread;
    public float FireSpeed { get { return fireSpeed; } }
    public float FireInterval { get { return fireInterval; } }
    public float ReloadTime { get { return reloadTime; } }
    public float Distance { get { return distance; } }
    public int MaxAmmoCount { get { return maxAmmoCount; } }

    public AmmoType AmmoType { get { return ammoType; } }

    public FireBehaviorType FireBehaviorType { get { return fireBehaviorType; } }
    public int OneShotFireCount { get { return oneShotFireCount; } }
    public float FireSpread { get { return fireSpread; } }
}
