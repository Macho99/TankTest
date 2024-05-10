using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EquipWeaponSpace { OneHand, TwoHands, Size };
public enum WeaponType { Main, Sub, Mily, Throw }

public abstract class WeaponItemSO : EquipmentItemSO
{
    [SerializeField] protected Sprite itemDetailIcon;
    [SerializeField] protected EquipWeaponSpace useHandSpace;
    [SerializeField] protected WeaponType weaponType;
    [SerializeField, Range(0, 200)] protected int damage;


    public WeaponType GetWeaponType() { return weaponType; }
}

public abstract class WeaponInstance : EquipmentItemInstance
{
    public WeaponInstance(ItemSO itemData, int count = 1) : base(itemData, count)
    {

    }


}