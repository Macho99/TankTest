using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasMask : EquipmentItem
{
    private int currentProdutionAmount;
    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            currentProdutionAmount = ((GasMaskSO)itemData).GasProtectionAmount;
        }
    }
    public override void Equip(PlayerController owner)
    {
        base.Equip(owner);

    }
}
