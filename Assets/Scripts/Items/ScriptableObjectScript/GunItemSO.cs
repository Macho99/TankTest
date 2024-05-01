using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GunType { AssultRiple, SniperRiple, Revolver, RocketLauncher, Shotgun }
public enum BulletType { FiveMilymeter, }
[CreateAssetMenu(fileName = "New Gun", menuName = "SO/Item/Weapon/Gun")]
public class GunItemSO : WeaponItemSO
{

    [SerializeField, Range(0, 500f)] private float fireSpeed;
    [SerializeField, Range(0, 5f)] private float fireInterval;
    [SerializeField, Range(0, 100f)] private float distance;
    [SerializeField, Range(0, 50)] private int maxBulletCount;
    [SerializeField, Range(0f, 10f)] private float reloadTime;
    [SerializeField, Range(0f, 10f)] private float ReboundHealthSpeed;


    public float FireSpeed { get { return fireSpeed; } }
    public float FireInterval { get { return fireInterval; } }
    public float ReloadTime { get { return reloadTime; } }
    public float Distance { get { return distance; } }
    public int MaxBulletCount { get { return maxBulletCount; } }
}
