using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoItemSO : MiscItemSO
{


    public override ItemInstance CreateItemData(int count = 1)
    {
        throw new System.NotImplementedException();
    }
}
public class AmmoItemInstance : MiscItemInstance
{
    public AmmoItemInstance(ItemSO itemData) : base(itemData)
    {

    }
}