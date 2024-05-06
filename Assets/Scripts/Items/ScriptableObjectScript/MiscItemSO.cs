using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Misc", menuName = "SO/ItemSO/Misc")]
public class MiscItemSO : ItemSO
{
    public override ItemInstance CreateItemData(int count = 1)
    {
        return new MiscItemInstance(this);
    }
}
public class MiscItemInstance : ItemInstance
{
    public MiscItemInstance(ItemSO itemData) : base(itemData)
    {

    }

    public override Item CreateItem(NetworkRunner runner, int count = 1)
    {
        Item item = runner.Spawn(itemObject);

        item.Init(this, count);
        item.name = this.itemData.ItemName;
        return item;
    }
}

