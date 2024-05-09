using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Misc", menuName = "SO/ItemSO/Misc")]
public class MiscItemSO : ItemSO
{
    public override ItemInstance CreateItemData(int count = 1)
    {
        return new MiscItemInstance(this, count);
    }
}
public class MiscItemInstance : ItemInstance
{
    public MiscItemInstance(ItemSO itemData, int count = 1) : base(itemData, count)
    {

    }
    public override ItemInstance Clone()
    {
        MiscItemInstance cloneItem = new MiscItemInstance(this.ItemData, this.Count);

        return cloneItem;
    }
    public override Item CreateNetworkItem(NetworkRunner runner, int count = 1)
    {
        Item item = runner.Spawn(itemObject);

        item.Init(count);
        item.name = this.itemData.ItemName;
        return item;
    }
}

