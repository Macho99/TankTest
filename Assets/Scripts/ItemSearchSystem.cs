using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSearchSystem : NetworkBehaviour
{

    public int MaxCount { get { return maxCount; } }
    private int maxCount;

    private Item[] itemSearchData;
    public event Action<int, Item> onUpdate;
    private Inventory inventory;
    private InteractItemBox interactItemBox;


    private void Awake()
    {
        maxCount = 40;
    }
    public void Init(Inventory inventory)
    {
        this.inventory = inventory;
    }
    public int FindEmptyItemSlot(Item item)
    {
        return 0;
    }
    public void UpdateItem()
    {
        itemSearchData = interactItemBox.items.ToArray();

        //ui 업데이트하기
        for (int i = 0; i < itemSearchData.Length; i++)
        {

            onUpdate?.Invoke(i, itemSearchData[i]);
        }
    }
    public bool AddSearchItem(InteractItemBox itemBox)
    {
        if (interactItemBox != null)
            return false;

        interactItemBox = itemBox;
        itemBox.onUpdate += UpdateItem;
        UpdateItem();

        return true;
    }
    public void ClearSearchItem()
    {
        if (interactItemBox == null) return;

        interactItemBox.onUpdate -= UpdateItem;
        for (int i = 0; i < itemSearchData.Length; i++)
        {
            onUpdate?.Invoke(i, null);
        }
        Array.Clear(itemSearchData, 0, itemSearchData.Length);
        interactItemBox = null;

    }
    public void AcquisitionItem(int index)
    {
        if (interactItemBox == null)
        {
            Debug.Log("interactItemBox null");
            return;
        }

        interactItemBox.RPC_AcquisitionItem(inventory, index);


    }

}
