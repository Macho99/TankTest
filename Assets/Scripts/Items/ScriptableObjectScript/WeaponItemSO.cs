using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EquipWeaponSpace { LeftHand, RightHand, All, Size };

[CreateAssetMenu(fileName = "New Weapon", menuName = "SO/Item/Weapon")]
public class WeaponItemSO : EquipmentItemSO
{
    [SerializeField] protected EquipWeaponSpace mainSpace;

}
