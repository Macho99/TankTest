using Fusion;
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
    [Networked, Capacity(50)] private NetworkArray<Item> items { get; }

    [Networked] private int Weight { get; set; }
    [Networked] private float MaxWeight { get; set; }


    private void Awake()
    {

    }
    public void ChangedItem()
    {

        
    }

    public override void Spawned()
    {
        Weight = 0;
        MaxWeight = 100f;


    }

    public void AddItem(ItemInstance newItem)
    {


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
