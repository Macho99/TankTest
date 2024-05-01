using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EquipWeaponSpace { OneHand,TwoHands, Size };

public class WeaponItemSO : EquipmentItemSO
{
    [SerializeField] protected Sprite itemDetailIcon;
    [SerializeField] protected EquipWeaponSpace mainSpace;
    [SerializeField] protected EquipWeaponSpace useHandSpace;
    [SerializeField,Range(0,200)] protected int damage;
}
