using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class ItemSearchSystem : NetworkBehaviour
{

    public int MaxCount { get { return maxCount; } }
    private int maxCount;
    [Networked, Capacity(50)] private NetworkArray<Item> searchItems { get; }
    public event Action<int, ItemInstance> onUpdate;
    private Inventory inventory;
    private void Awake()
    {
        maxCount = 10;
    }
    public void Init(Inventory inventory)
    {
        this.inventory = inventory;
    }
    public bool AddSearchItem(NetworkArray<Item> items)
    {



        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                Debug.Log(items[i].ItemInstance);
            }
            //searchItems.Set(i, items[i]);

            //if (searchItems[i] != null)
            //    Debug.Log(searchItems[i].ItemInstance.ItemData.ItemName);
        }

        //if (FindEqualSearchItemList(items, out int listIndex))
        //{
        //    return false;
        //}

        //int EmptyIndex = FindEmptySpace();
        //if (EmptyIndex == -1)
        //{
        //    Debug.Log("공간이 부족합니다");
        //    return false;
        //}

        //searchDatas[EmptyIndex] = items;

        //for (int i = 0; i < searchDatas[EmptyIndex].itemList.Count; i++)
        //{
        //    onUpdate?.Invoke(i + EmptyIndex, searchDatas[EmptyIndex].itemList[i]);
        //}
        //searchDatas[EmptyIndex].onUpdate -= UpdateItem;
        //searchDatas[EmptyIndex].onUpdate += UpdateItem;

        return true;
    }
    public void UpdateItem(ItemSearchData searchData)
    {
        //for (int i = 0; i < searchDatas.Length; i++)
        //{
        //    if (searchData == searchDatas[i])
        //    {
        //        searchDatas[i] = searchData;
        //        for (int j = 0; j < searchDatas[i].itemList.Count; j++)
        //        {
        //            onUpdate?.Invoke(i + j, searchDatas[i].itemList[j]);
        //        }
        //        Debug.Log("update");
        //        return;
        //    }
        //}
    }
    private int FindEmptySpace()
    {
        //for (int i = 0; i < searchDatas.Length; i++)
        //{
        //    if (searchDatas[i] == null)
        //        return i;
        //}

        return -1;
    }
    private bool FindEqualSearchItemList(ItemSearchData items, out int index)
    {
        //for (int i = 0; i < searchDatas.Length; i++)
        //{
        //    if (searchDatas[i] == null)
        //        continue;

        //    if (searchDatas[i] == items)
        //    {
        //        index = i;
        //        return true;
        //    }
        //}

        index = -1;
        return false;
    }

    public void ClearsearchItem(ItemSearchData items)
    {
        //if (FindEqualSearchItemList(items, out int index))
        //{
        //    for (int j = 0; j < searchDatas[index].itemList.Count; j++)
        //    {
        //        onUpdate?.Invoke(j, null);
        //    }
        //    searchDatas[index] = null;
        //}
    }


    public void AcquisitionItem(int newindex)
    {
        //for (int i = 0; i < searchDatas.Length; i++)
        //{
        //    if (searchDatas[i] == null)
        //        continue;

        //    for (int j = 0; j < searchDatas[i].itemList.Count; j++)
        //    {
        //        if (newindex == (i + j))
        //        {
        //            ItemInstance cloneItem = searchDatas[i].itemList[j].Clone();
        //            inventory.AddItem(cloneItem);
        //            searchDatas[i].RemoveItemData(j);
        //            onUpdate?.Invoke(i + j, null);
        //            break;
        //        }
        //    }
        //}


    }

}
