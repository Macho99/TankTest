using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class WeaponItemSO : EquipmentItemSO
{
    [SerializeField] protected Sprite itemDetailIcon;
<<<<<<< HEAD
    [SerializeField] protected EquipWeaponSpace useHandSpace;
    [SerializeField] protected WeaponType weaponType;
    [SerializeField] protected WeaponAnimLayer weaponAnimLayer;
    [SerializeField, Range(0, 200)] protected int damage;


    public WeaponType GetWeaponType() { return weaponType; }
    public Sprite GetItemDetailIcon() { return itemDetailIcon; }

    public WeaponAnimLayer GetWeaponAnimLayer() { return weaponAnimLayer; }
=======
    [SerializeField] protected WeaponAnimLayerType animLayerType;
    [SerializeField, Range(0, 200)] protected int damage;

    public WeaponAnimLayerType AnimLayerType { get { return animLayerType; } }
    public Sprite ItemDetailIcon { get { return itemDetailIcon; } }
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
}

public abstract class WeaponInstance : EquipmentItemInstance
{
    public WeaponInstance(ItemSO itemData, int count = 1) : base(itemData, count)
    {

    }


}