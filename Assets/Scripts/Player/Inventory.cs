using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ItemStruct : INetworkStruct
{
    public NetworkBool isEmpty;
    public int itemID;
    public int count;

}
public class Inventory : NetworkBehaviour
{
    [Networked, Capacity(50), OnChangedRender(nameof(OnChangeItem))] private NetworkArray<Item> netItems { get; }

    private int maxCount;
    [Networked] public float Weight { get; private set; }
    [Networked] public float MaxWeight { get; private set; }


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

            MaxWeight = 1000f;
        }
        OnChangeItem();
    }
    public override void Render()
    {
        if (Object.IsProxy)
        {
            for (int i = 0; i < maxCount; i++)
            {
                if (netItems[i] != null)
                {
                    Debug.Log(netItems[i].name);
                }
            }
        }
    }
    public void AddItem(Item newItem)
    {
        for (int i = 0; i < netItems.Length; i++)
        {
            if (netItems[i] == null)
            {
                if (newItem.ItemData.Weight + Weight > MaxWeight)
                    continue;

                netItems.Set(i, newItem);
                Weight += netItems[i].ItemData.Weight;
                netItems[i].SetParent(this.transform);
                netItems[i].gameObject.SetActive(false);
                break;
            }
        }
    }
    public void MoveItem(Item newItem)
    {
        for (int i = 0; i < netItems.Length; i++)
        {
            if (netItems[i] == null)
            {
                netItems.Set(i, newItem);
                netItems[i].SetParent(this.transform);
                netItems[i].gameObject.SetActive(false);
                netItems.Set(i, newItem);
                Weight += netItems[i].ItemData.Weight;
                netItems[i].SetParent(this.transform);
                onItemUpdate?.Invoke(i, netItems[i]);
                break;
            }
        }
    }
    public Item GetItem(int index)
    {
        if (netItems[index] == null)
        {
            Debug.Log("존재하지않음");
            return null;
        }


        return netItems[index];
    }
    public int GetItemIndex(Item item)
    {
        if (item == null)
        {
            Debug.Log("존재하지않음");
            return -1;
        }

        int index = Array.FindIndex(netItems.ToArray(), (Item item2) => { return item == item2; });
        return index;
    }
    public void InsideMoveItem(Item item)
    {
        if (item == null)
            return;

        for (int i = 0; i < netItems.Length; i++)
        {
            if (netItems[i] == null)
            {
                netItems.Set(i, item);
                netItems[i].SetParent(this.transform);
                onItemUpdate?.Invoke(i, netItems[i]);
                break;
            }
        }

    }

    public void InsidePullItem(Item item)
    {
        if (item == null)
            return;

        for (int i = 0; i < netItems.Length; i++)
        {
            if (netItems[i] == null)
                continue;

            if (item == netItems[i])
            {
                InsidePullItem(i);
                onItemUpdate?.Invoke(i, null);
                return;
            }

        }


    }
    public Item InsidePullItem(int index)
    {

        if (netItems[index] == null)
        {
            Debug.Log("존재하지않음");
            return null;
        }

        Item pullItem = netItems[index];
        netItems.Set(index, null);
        onItemUpdate?.Invoke(index, null);
        return pullItem;
    }
    public void OnChangeItem()
    {
        for (int i = 0; i < netItems.Length; i++)
        {
            //if (netItems[i] != items[i])
            //{
            //    items[i].SetParent(this.transform);
            //    netItems[i].SetParent(this.transform);
            //}
            //onItemUpdate?.Invoke(i, items[i]);
        }
    }

}
