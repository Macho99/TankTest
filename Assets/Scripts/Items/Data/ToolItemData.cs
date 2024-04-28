using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ToolItemType { Hammer, Axe, Size }

[Serializable]
public class ToolItemData : ItemData
{
    [SerializeField] private ToolItemType toolItemType;


    public ToolItemType ToolItemType { get { return toolItemType; } }

}
