using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : NetworkBehaviour
{

    private Item[] items;
    [Networked] private int count { get; set; }
    [Networked] private int MaxCount { get; set; }

    private void Awake()
    {

    }

    public override void Spawned()
    {
        count = 0;
        MaxCount = 20;
        items = new Item[MaxCount];

    }
    public void AddItem(Item newItem)
    {

        if (count >= MaxCount)
        {
            return;
        }

        for (int i = 0; i < MaxCount; i++)
        {
            if (items[i] == null)
            {
                items[i] = newItem;
                items[i].transform.SetParent(transform);
                items[i].gameObject.SetActive(false);
                count++;
                break;
            }
        }


    }
}
