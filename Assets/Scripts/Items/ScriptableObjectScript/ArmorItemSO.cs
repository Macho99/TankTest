using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum ArmorType { Helmet, Armor, Mask }
public class ArmorItemSO : EquipmentItemSO
{
    [SerializeField] private ArmorType armorType;


    public override ItemInstance CreateItemData(int count = 1)
    {
        throw new System.NotImplementedException();
    }
}
