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
    [Networked, Capacity(50), OnChangedRender(nameof(OnChangeItem))] public NetworkArray<Item> netItems { get; }

    private int maxCount;
    [Networked] public float Weight { get; private set; }
    [Networked] public float MaxWeight { get; private set; }
    [SerializeField] private PlayerStat playerStat;

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
    public void UseItem(int index)
    {
        if (netItems[index] is UseItem)
        {
            ((UseItem)netItems[index]).Use(playerStat);
            if (netItems[index] == null)
            {
                netItems.Set(index, null);
                onItemUpdate?.Invoke(index, null);
                return;

            }
            onItemUpdate?.Invoke(index, netItems[index]);
        }
    }
    public void AddItem(Item newItem)
    {
        if (newItem.ItemData.IsStackable)
        {
            int count = newItem.currentCount;

            Item[] items = Array.FindAll(netItems.ToArray(), (item) =>
            {
                if (item == null)
                    return false;

                return (item.ItemData.ItemID == newItem.ItemData.ItemID) && item.ItemData.MaxCount > item.currentCount;
            });
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].AddItemCount(count, out int remainingCount))
                {

                    Weight += items[i].ItemData.Weight * (count - remainingCount);
                    count = remainingCount;
                    if (count <= 0)
                    {
                        newItem.ChangeItemCount(count);


                        netItems[i].SetParent(this.transform);
                        netItems[i].gameObject.SetActive(false);

                        onItemUpdate?.Invoke(GetItemIndex(items[i]), items[i]);

                        return;
                    }
                }
            }
            if (count > 0)
            {
                newItem.ChangeItemCount(count);
                int index = Array.FindIndex(netItems.ToArray(), (item) => { return item == null; });
                netItems.Set(index, newItem);
                netItems[index].SetParent(this.transform);
                netItems[index].gameObject.SetActive(false);
                Weight += netItems[index].ItemData.Weight * count;
                onItemUpdate?.Invoke(index, netItems[index]);
            }
            return;

        }
        for (int i = 0; i < netItems.Length; i++)
        {
            if (netItems[i] == null)
            {

                if (newItem.ItemData.Weight * newItem.currentCount + Weight > MaxWeight)
                    continue;

                netItems.Set(i, newItem);
                Weight += netItems[i].ItemData.Weight * netItems[i].currentCount;
                netItems[i].SetParent(this.transform);
                netItems[i].gameObject.SetActive(false);
                onItemUpdate?.Invoke(i, netItems[i]);
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
                netItems[i].RPC_SetActive(false);
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
            onItemUpdate?.Invoke(i, netItems[i]);
        }
    }

    public int GetTotalAmmoItemCount(Gun weapon)
    {

        Item[] items = Array.FindAll(netItems.ToArray(), (item) =>
         {
             if (item == null || item is Ammo == false)
                 return false;
             return ((GunItemSO)weapon.ItemData).AmmoType == ((AmmoItemSO)item.ItemData).AmmoType;

         });

        int maxCount = 0;
        foreach (Ammo ammo in items)
        {
            maxCount += ammo.currentCount;
        }

        return maxCount;
    }
    public Ammo GetAmmoItems(int index, AmmoType ammoType)
    {

        if (netItems[index] == null)
        {
            return null;
        }
        if (netItems[index] is Ammo == false)
            return null;



        if (((AmmoItemSO)netItems[index].ItemData).AmmoType != ammoType)
            return null;


        return (Ammo)netItems[index];


    }
    public void PullAmmoItemCount(AmmoType ammoType, int count)
    {
        for (int i = 0; i < netItems.Length; i++)
        {
            if (netItems[i] == null)
                continue;

            if (netItems[i] is Ammo == false)
                continue;

            netItems[i].TakeOutItemCount(count, out int remaingCount);
            count = remaingCount;
            if (netItems[i] == null)
                netItems.Set(i, null);

            onItemUpdate?.Invoke(i, netItems[i]);
            if (count <= 0)
            {
                return;
            }

        }

    }



}
