using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GunType { Rifle, SniperRiple, Revolver, RocketLauncher, Shotgun }

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

    public float FireSpeed { get { return fireSpeed; } }
    public float FireInterval { get { return fireInterval; } }
    public float ReloadTime { get { return reloadTime; } }
    public float Distance { get { return distance; } }
    public int MaxAmmoCount { get { return maxAmmoCount; } }

    public AmmoType AmmoType { get { return ammoType; } }

    public override ItemInstance CreateItemData(int count = 1)
    {
        return new GunItemInstance(this);
    }
}
public class GunItemInstance : WeaponInstance
{
    private int currentAmmo;
    public GunItemInstance(ItemSO itemData, int count = 1, int ammo = 0) : base(itemData, count)
    {
        currentAmmo = ammo;
    }
    public override ItemInstance Clone()
    {
        GunItemInstance cloneItem = new GunItemInstance(this.ItemData, this.Count, this.currentAmmo);

        return cloneItem;
    }
    public override Item CreateNetworkItem(NetworkRunner runner, int count = 1)
    {
        Item item = runner.Spawn(itemObject);

        item.Init(count);
        item.name = this.itemData.ItemName;
        return item;
    }
}