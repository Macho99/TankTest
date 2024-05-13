using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : NetworkBehaviour
{
    [SerializeField] protected ItemSO itemData;

    [Networked, HideInInspector] public int currentCount { get; set; }
    public ItemSO ItemData { get { return itemData; } }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }


    public override void Spawned()
    {
        gameObject.SetActive(false);
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public int ItemCount { get { return currentCount; } }

    public override void Render()
    {

    }

    public void Init(int count)
    {
        this.currentCount = count;
    }

}
