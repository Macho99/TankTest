using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum ArmorType { Helmet, Armor }
[CreateAssetMenu(fileName = "New Armor", menuName = "SO/ItemSO/Armor")]
public class ArmorItemSO : EquipmentItemSO
{
    [SerializeField] private ArmorType armorType;
    [SerializeField] private float reductionAmount;

}


