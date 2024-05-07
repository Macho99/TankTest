using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EquipWeaponSpace { OneHand,TwoHands, Size };

public abstract class WeaponItemSO : EquipmentItemSO
{
    [SerializeField] protected Sprite itemDetailIcon;
    [SerializeField] protected EquipWeaponSpace useHandSpace;
    [SerializeField,Range(0,200)] protected int damage;
}

public abstract class WeaponInstance : EquipmentItemInstance
{
    public WeaponInstance(ItemSO itemData, int count = 1) : base(itemData,count)
    {

    }

 
}