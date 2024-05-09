using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using static UnityEditor.Progress;

public struct ItemStruct : INetworkStruct
{
    public NetworkBool isEmpty;
    public int itemID;
    public int count;

}
public class Inventory : NetworkBehaviour
{
    [Networked, Capacity(50)] private NetworkArray<Item> items { get; }

    private int maxCount;
    [Networked] private float Weight { get; set; }
    [Networked] private float MaxWeight { get; set; }


    public int MaxCount { get { return maxCount; } }
    public event Action<int, Item> onItemUpdate;
    private void Awake()
    {
        maxCount = 50;
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            Weight = 0f;
            MaxWeight = 100f;
        }
    }
    public override void Render()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                Debug.Log($"{Object.Id}{items[i].name}");

            }
        }
        Debug.Log($"{Object.Id}{Weight}");
    }
    public void AddItem(Item newItem)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                if (newItem.ItemData.Weight + Weight > MaxWeight)
                    continue;

                items.Set(i, newItem);
                Weight += items[i].ItemData.Weight;
                Debug.Log($"{Object.Id}{items[i].name}");
                items[i].SetParent(this.transform);
                onItemUpdate?.Invoke(i, items[i]);
                break;
            }
        }

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetParent(Item item)
    {

    }
}
