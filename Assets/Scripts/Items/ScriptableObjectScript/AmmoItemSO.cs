using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum AmmoType { _5_56mm, _7_62mm, Gauge12, Rocket, Size }

[CreateAssetMenu(fileName = "New Ammo", menuName = "SO/ItemSO/Ammo")]

public class AmmoItemSO : MiscItemSO
{
    [SerializeField] private AmmoType ammoType;

    public AmmoType AmmoType { get { return ammoType; } }

}
