using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : NetworkBehaviour
{
    [SerializeField] protected ItemSO itemData;

    [Networked] public int currentCount { get; set; }
    [Networked, OnChangedRender(nameof(OnChangeParent))] public NetworkBehaviour owner { get; set; }
    public ItemSO ItemData { get { return itemData; } }

    public void SetParent(NetworkBehaviour parent)
    {
        owner = parent;
    }
    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
    }

    public void OnChangeParent()
    {
        transform.SetParent(owner?.transform);
        Debug.Log("ü����");
    }
    public override void Spawned()
    {
        OnChangeParent();
        //gameObject.SetActive(false);
    }
    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
    public int ItemCount { get { return currentCount; } }

    public override void Render()
    {
        if (owner != null)
            Debug.Log(owner.gameObject.name);
    }

    public void Init(int count)
    {
        this.currentCount = count;
    }
}
