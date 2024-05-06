using Fusion;
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
    [SerializeField] protected int maxCount;
    [SerializeField] protected Item itemObject;

    public abstract ItemInstance CreateItemData(int count = 1);
    public Sprite ItemIcon { get { return itemIcon; } }
    public string ItemName { get { return itemName; } }
    public float Weight { get { return weight; } }
    public int MaxCount { get { return maxCount; } }
    public Item ItemObject { get { return itemObject; } }

}
public abstract class ItemInstance
{
    protected ItemSO itemData;
    protected Item itemObject;


    public ItemSO ItemData { get { return itemData; } }
    public ItemInstance(ItemSO itemData)
    {
        this.itemData = itemData;
        this.itemObject = itemData.ItemObject;
    }
    public abstract Item CreateItem(NetworkRunner runner,int count = 1);
}