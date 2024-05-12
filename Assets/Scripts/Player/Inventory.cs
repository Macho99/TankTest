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
    [Networked, Capacity(50), OnChangedRender(nameof(OnChangeItem))] private NetworkArray<Item> items { get; }

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
            MaxWeight = 500f;
        }
    }
    public override void Render()
    {
        if (Object.IsProxy)
        {
            for (int i = 0; i < maxCount; i++)
            {
                if (items[i] != null)
                {
                    Debug.Log(items[i].name);
                }
            }
        }
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
                break;
            }
        }
    }
    public Item GetItem(int index)
    {
        if (items[index] == null)
        {
            Debug.Log("존재하지않음");
            return null;
        }
        return items[index];
    }
    public Item InsidePullItem(int index)
    {
        if (items[index] == null)
        {
            Debug.Log("존재하지않음");
            return null;
        }

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
    }
    public void OnChangeItem()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                items[i].SetParent(this.transform);
               
            }
            onItemUpdate?.Invoke(i, items[i]);
        }
    }

}
