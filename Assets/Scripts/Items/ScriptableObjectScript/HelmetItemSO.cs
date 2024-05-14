using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Helmet", menuName = "SO/ItemSO/Helmet")]
public class HelmetItemSO : EquipmentItemSO
{
    [SerializeField] private int increaseDefense;

    public override ItemInstance CreateItemData(int count = 1)
    {
        return null;
    }
}
