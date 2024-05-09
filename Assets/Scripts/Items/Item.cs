using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : NetworkBehaviour
{
    [SerializeField] protected ItemSO itemData;

    [Networked] protected int currentCount { get; set; }
    //[Networked, OnChangedRender(nameof(Active))] public bool IsActive { get; set; }
    //private void Active()
    //{
    //    if (IsActive)
    //    {
    //        gameObject.SetActive(true);
    //    }
    //    else
    //    {
    //        gameObject.SetActive(false);
    //    }
    //}
    public override void Spawned()
    {
        gameObject.SetActive(false);
    }
    public void SetActive(bool isActive)
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
