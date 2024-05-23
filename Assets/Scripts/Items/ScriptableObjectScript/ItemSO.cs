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

    public int ItemID { get { return itemId; } }
    public Sprite ItemIcon { get { return itemIcon; } }
    public string ItemName { get { return itemName; } }
    public float Weight { get { return weight; } }
    public int MaxCount { get { return maxCount; } }

    public bool IsStackable { get { return isStackable; } }

}
