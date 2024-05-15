using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : NetworkBehaviour
{
    [SerializeField] protected ItemSO itemData;

    [Networked, OnChangedRender(nameof(UpdateCount))] public int currentCount { get; set; } = 1;
    public ItemSO ItemData { get { return itemData; } }


    public void ChangeItemCount(int count)
    {
        currentCount = count;
        if (currentCount == 0)
        {
            if (HasStateAuthority)
            {
                Runner.Despawn(Object);
            }
        }
    }
    public bool AddItemCount(int count, out int remainingCount)
    {
        if (!itemData.IsStackable)
        {
            remainingCount = -1;
            return false;
        }

        int maxCount = itemData.MaxCount;
        if (currentCount == maxCount)
        {
            remainingCount = count;
            return true;
        }
        if (currentCount + count > maxCount)
        {
            remainingCount = currentCount + count - maxCount;
            currentCount = maxCount;

            return true;
        }
        else
        {
            currentCount += count;
            remainingCount = 0;

            return true;
        }

    }
    public void UpdateCount()
    {
        Debug.Log(currentCount);
        if (currentCount <= 0)
        {
            if (HasStateAuthority)
            {

                Debug.Log("»èÁ¦");
                Runner.Despawn(Object);
                return;
            }
        }
    }
    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }


    public override void Spawned()
    {
        gameObject.SetActive(false);
        Debug.Log(currentCount);
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }



    public override void Render()
    {

    }

    public void Init(int count)
    {
        this.currentCount = count;
    }


}
