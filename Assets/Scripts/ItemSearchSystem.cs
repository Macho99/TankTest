using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEditor.Search;
using UnityEngine;

public class ItemSearchSystem : NetworkBehaviour
{

    public int MaxCount { get { return maxCount; } }
    private int maxCount;
    [Networked, Capacity(20)] private NetworkArray<Item> searchItems { get; }
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
    public int FindEmptyItemSlot(Item item)
    {
        return 0;
    }
    public bool AddSearchItem(NetworkArray<Item> searchData)
    {


        for (int i = 0; i < searchData.Length; i++)
        {
            if (searchItems[i] == null)
            {
                if (searchData[i] != null)
                {
                    searchItems.Set(i, searchData[i]);
                    Debug.Log(searchItems[i].name);
                }
            }
        }
        for (int i = 0; i < searchItems.Length; i++)
        {
            if (searchItems[i] != null)
            {
                Debug.Log(searchItems[i].name);
            }
            //    continue;
            //else
            //{
            //    Debug.Log(searchItems[i].name);
            //}

            //for (int j = 0; j < searchData.Length; j++)
            //{
            //    if (searchData[j] == null)
            //        continue;

            //    Debug.Log("searchItems : " + searchItems[i] + "searchData : " + searchData[j]);

            //    if (searchData[j] == searchItems[i])
            //    {
            //        Debug.Log("같음");
            //    }
            //}
        }



        //if (FindEqualSearchItemList(searchData, out int listIndex))
        //{
        //    return false;
        //}

        //int EmptyIndex = FindEmptySpace();
        //if (EmptyIndex == -1)
        //{
        //    Debug.Log("공간이 부족합니다");
        //    return false;
        //}

        //searchDatas[EmptyIndex] = searchData;

        //for (int j = 0; j < searchDatas[EmptyIndex].itemList.Count; j++)
        //{
        //    onUpdate?.Invoke(j + EmptyIndex, searchDatas[EmptyIndex].itemList[j]);
        //}
        //searchDatas[EmptyIndex].onUpdate -= UpdateItem;
        //searchDatas[EmptyIndex].onUpdate += UpdateItem;

        return true;
    }
    //public void UpdateItem(ItemSearchData searchData)
    //{
    //    for (int j = 0; j < searchDatas.Length; j++)
    //    {
    //        if (searchData == searchDatas[j])
    //        {
    //            searchDatas[j] = searchData;
    //            for (int j = 0; j < searchDatas[j].itemList.Count; j++)
    //            {
    //                onUpdate?.Invoke(j + j, searchDatas[j].itemList[j]);
    //            }
    //            Debug.Log("update");
    //            return;
    //        }
    //    }
    //}
    //private int FindEmptySpace()
    //{
    //    for (int j = 0; j < searchDatas.Length; j++)
    //    {
    //        if (searchDatas[j] == null)
    //            return j;
    //    }

    //    return -1;
    //}
    //private bool FindEqualSearchItemList(ItemSearchData items, out int index)
    //{
    //    for (int j = 0; j < searchDatas.Length; j++)
    //    {
    //        if (searchDatas[j] == null)
    //            continue;

    //        if (searchDatas[j] == items)
    //        {
    //            index = j;
    //            return true;
    //        }
    //    }

    //    index = -1;
    //    return false;
    //}

    //public void ClearsearchItem(ItemSearchData items)
    //{
    //    if (FindEqualSearchItemList(items, out int index))
    //    {
    //        for (int j = 0; j < searchDatas[index].itemList.Count; j++)
    //        {
    //            onUpdate?.Invoke(j, null);
    //        }
    //        searchDatas[index] = null;
    //    }
    //}


    //public void AcquisitionItem(int newindex)
    //{
    //    for (int j = 0; j < searchDatas.Length; j++)
    //    {
    //        if (searchDatas[j] == null)
    //            continue;

    //        for (int j = 0; j < searchDatas[j].itemList.Count; j++)
    //        {
    //            if (newindex == (j + j))
    //            {
    //                ItemInstance cloneItem = searchDatas[j].itemList[j].Clone();
    //                inventory.AddItem(cloneItem);
    //                searchDatas[j].RemoveItemData(j);
    //                onUpdate?.Invoke(j + j, null);
    //                break;
    //            }
    //        }
    //    }


    //}

}
