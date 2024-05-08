using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : NetworkBehaviour
{
    protected ItemInstance itemInstance;


    [Networked] protected int currentCount { get; set; }

    public override void Spawned()
    {

    }
    public int ItemCount { get { return currentCount; } }
    public ItemInstance ItemInstance { get { return itemInstance; } }

    public override void FixedUpdateNetwork()
    {
        Debug.Log(gameObject.name + currentCount);
    }

    public void Init(ItemInstance itemData, int count)
    {
        this.itemInstance = itemData;
        this.currentCount = count;
    }
}
