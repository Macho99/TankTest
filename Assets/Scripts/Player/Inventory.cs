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

    private Item[] items;
    private int maxCount;
    [Networked] public float Weight { get; private set; }
    [Networked] public float MaxWeight { get; private set; }


    public int MaxCount { get { return maxCount; } }
    public event Action<int, Item> onItemUpdate;
    private void Awake()
    {
        maxCount = 50;
        items = new Item[maxCount];

    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            Weight = 0f;
<<<<<<< HEAD
            MaxWeight = 500f;
=======
            MaxWeight = 1000f;
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
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

<<<<<<< HEAD
                items.Set(i, newItem);
                Weight += items[i].ItemData.Weight;
                items[i].SetParent(this.transform);
                items[i].gameObject.SetActive(false);
                break;
            }
        }
    }
    public void MoveItem(Item newItem)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items.Set(i, newItem);
                items[i].SetParent(this.transform);
                items[i].gameObject.SetActive(false);
=======
                netItems.Set(i, newItem);
                Weight += netItems[i].ItemData.Weight;
                netItems[i].SetParent(this.transform);
                onItemUpdate?.Invoke(i, netItems[i]);
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
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
<<<<<<< HEAD
        return items[index];
=======

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
                netItems[i].SetActive(false);
                onItemUpdate?.Invoke(i, netItems[i]);
                break;
            }
        }

>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
    }
    public Item InsidePullItem(int index)
    {
        if (netItems[index] == null)
        {
            Debug.Log("존재하지않음");
            return null;
        }

<<<<<<< HEAD
        Item pullItem = items[index];
       // Weight -= items[index].ItemData.Weight;


        items.Set(index, null);
        return pullItem;
    }
    public int GetItemIndex(Item item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
                continue;

            if (item.Equals(items[i]))
                return i;
        }
        return -1;
=======

        Item pullItem = netItems[index];
        Weight -= pullItem.ItemData.Weight;
        netItems.Set(index, null);
        onItemUpdate?.Invoke(index, null);
        return pullItem;
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
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
    }
    public void OnChangeItem()
    {
        for (int i = 0; i < netItems.Length; i++)
        {
            if (netItems[i] != items[i])
            {
<<<<<<< HEAD
                items[i].SetParent(this.transform);
               
=======
                // items[i] = netItems[i];

                netItems[i].SetParent(this.transform);
>>>>>>> 81b7febcd4941e8c50244fa3ba95f413730222e4
            }
            onItemUpdate?.Invoke(i, items[i]);
        }
    }

}
