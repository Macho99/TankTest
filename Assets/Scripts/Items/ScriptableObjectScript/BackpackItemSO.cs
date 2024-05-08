using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Backpack", menuName = "SO/ItemSO/Backpack")]
public class BackpackItemSO : EquipmentItemSO
{
    [SerializeField] private float weightGain;


    public override ItemInstance CreateItemData(int count = 1)
    {
        return new BackpackItemInstance(this);
    }

}
public class BackpackItemInstance : EquipmentItemInstance
{
    public BackpackItemInstance(ItemSO itemData, int count = 1) : base(itemData, count)
    {
    }
    public override ItemInstance Clone()
    {
        BackpackItemInstance cloneItem = new BackpackItemInstance(this.ItemData, this.Count);

        return cloneItem;
    }

    public override Item CreateNetworkItem(NetworkRunner runner, int count = 1)
    {
        Item item = runner.Spawn(itemObject);

        item.Init(this, count);
        return item;
    }
}
