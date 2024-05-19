using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New GasMask", menuName = "SO/ItemSO/GasMask")]
public class GasMaskSO : EquipmentItemSO
{
    [SerializeField] private int gasProtectionAmount;

    public int GasProtectionAmount { get { return gasProtectionAmount; } }


}
