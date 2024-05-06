using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : NetworkBehaviour
{

    private List<ItemInstance> items = new List<ItemInstance>();
    [Networked] private int Weight { get; set; }
    [Networked] private float MaxWeight { get; set; }


    private void Awake()
    {

    }


    public override void Spawned()
    {
        Weight = 0;
        MaxWeight = 100f;
        items = new List<ItemInstance>();


    }

    public void AddItem(Item newItem)
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
