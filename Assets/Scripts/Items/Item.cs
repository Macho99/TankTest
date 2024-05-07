using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : NetworkBehaviour
{ 
    protected ItemInstance itemInstance;


    protected int currentCount;


    public int ItemCount { get { return currentCount; } }
    public ItemInstance ItemInstance { get { return itemInstance; } }


    public void Init(ItemInstance itemData, int count)
    {
        this.itemInstance = itemData;
        this.currentCount = count;
    }
}
