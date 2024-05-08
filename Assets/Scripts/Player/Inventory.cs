using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public struct ItemStruct : INetworkStruct
{
    public NetworkBool isEmpty;
    public int itemID;
    public int count;

}
public class Inventory : NetworkBehaviour
{
    //[Networked, Capacity(50)] private NetworkArray<Item> items { get; }

    private List<ItemInstance> items;
    [Networked] private int Weight { get; set; }
    [Networked] private float MaxWeight { get; set; }

    public event Action<int> onItemUpdate;

    private void Awake()
    {
        items = new List<ItemInstance>();
    }

    public override void Spawned()
    {
        Weight = 0;
        MaxWeight = 100f;


    }

    //[Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void AddItem(ItemInstance newItem)
    {
        items.Add(newItem);
        onItemUpdate?.Invoke(items.Count - 1);

       //// Debug.Log(items[items.Count - 1].ItemData.ItemName);
    }
    public void RemoveItem(int index)
    {

    }


    private void Checkweight(Item item)
    {
        //float itemWeight = item.ItemData.Weight;


        //if (itemWeight + this.Weight > MaxWeight)
        //{

        //}

    }
}
