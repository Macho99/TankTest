using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemSO : ScriptableObject
{
    protected int itemId => GetInstanceID();
    [SerializeField] protected Sprite itemIcon;
    [SerializeField] protected string itemName;
    [SerializeField] protected float weight;
    [SerializeField, TextArea(4, 5)] protected string itemDescrition;
    [SerializeField] protected bool isStackable;
    [SerializeField] protected int maxCount;
    [SerializeField] protected Item itemObject;
    public abstract ItemInstance CreateItemData(int count = 1);

    public int ItemID { get { return itemId; } }
    public Sprite ItemIcon { get { return itemIcon; } }
    public string ItemName { get { return itemName; } }
    public float Weight { get { return weight; } }
    public int MaxCount { get { return maxCount; } }

    public bool IsStackable { get { return isStackable; } }
    public Item ItemObject { get { return itemObject; } }

}
[Serializable]
public abstract class ItemInstance
{
    protected ItemSO itemData;
    protected Item itemObject;

    private int count;
    public ItemSO ItemData { get { return itemData; } }

    public int Count { get { return count; } }
    public ItemInstance(ItemSO itemData, int count = 1)
    {
        this.itemData = itemData;
        this.itemObject = itemData.ItemObject;
        this.count = count;
    }

    public abstract ItemInstance Clone();
    //public abstract Item CreatItem(int count = 1);
    public abstract Item CreateNetworkItem(NetworkRunner runner, int count = 1);
}
public struct ItemTest : INetworkStruct
{
    public NetworkId owner;

}