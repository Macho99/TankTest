using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class WeaponItemSO : EquipmentItemSO
{
    [SerializeField] protected Sprite itemDetailIcon;

    [SerializeField] protected WeaponAnimLayerType animLayerType;
    [SerializeField, Range(0, 200)] protected int damage;

    public int Damage { get => damage; }
    public WeaponAnimLayerType AnimLayerType { get { return animLayerType; } }
    public Sprite ItemDetailIcon { get { return itemDetailIcon; } }
}

