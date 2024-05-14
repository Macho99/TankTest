using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public class PickupItem : InteractObject
{
    [SerializeField] private ItemSO itemData;

    public override void Spawned()
    {
        base.Spawned();
        if (HasStateAuthority)
        {
            DetectData.interactHint = "아이템 줍기";
        }



    }

    public override void StartInteraction()
    {
        throw new NotImplementedException();
    }
}
