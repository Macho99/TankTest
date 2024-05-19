using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New MeleeWeapon", menuName = "SO/ItemSO/Weapon/Melee")]
public class MeleeWeaponItemSO : WeaponItemSO
{
    [SerializeField, Range(0f, 2f)] private float attackSpeed = 1f;

}
