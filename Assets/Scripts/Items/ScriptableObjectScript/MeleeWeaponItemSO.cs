using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New MeleeWeapon", menuName = "SO/ItemSO/Weapon/Melee")]
public class MeleeWeaponItemSO : WeaponItemSO
{
    [SerializeField, Range(0f, 2f)] private float attackSpeed = 1f;

    public override ItemInstance CreateItemData(int count = 1)
    {
        return new MeleeWeaponItemInstance(this);
    }
}
public class MeleeWeaponItemInstance : WeaponInstance
{
    public MeleeWeaponItemInstance(ItemSO itemData) : base(itemData)
    {

    }

    public override Item CreateNetworkItem(NetworkRunner runner, int count = 1)
    {
        Item item = runner.Spawn(itemObject);

        item.Init(this, count);
        item.name = this.itemData.ItemName;
        return item;
    }
}
