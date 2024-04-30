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
}
