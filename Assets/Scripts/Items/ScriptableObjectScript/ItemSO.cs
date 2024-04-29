using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemSO : ScriptableObject
{
    [SerializeField] protected int itemId => GetInstanceID();
    [SerializeField] protected Sprite itemIcon;
    [SerializeField] protected string itemName;
    [SerializeField, TextArea(0, 5)] protected string itemDescrition;

}
