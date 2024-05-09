using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentType { Helmet, Mask, Armor, Backpack, MainWeapon, SubWeapon, MilyWeapon, ThrowWeapon }
public class Equipment : NetworkBehaviour
{

    [SerializeField] private Transform[] EquipmentSlots;



    public void SetEquipItem(Item item)
    {
            
    }
}
