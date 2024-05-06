using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSearchSystem : NetworkBehaviour
{

    List<ItemSearchData> searchDatas;

    public event Action<int, ItemInstance> onUpdate;

    private void Awake()
    {
        searchDatas = new List<ItemSearchData>();

    }
    public bool AddSearchItem(ItemSearchData items)
    {
        if (!FindEqualSearchItemList(items))
            return false;

        searchDatas.Add(items);
        int index = 0;
        for (int i = 0; i < searchDatas.Count; i++)
        {
            for (int j = 0; j < searchDatas[i].itemList.Count; j++)
            {
                onUpdate?.Invoke(index, searchDatas[i].itemList[j]);
                index++;
            }
        }

        return true;
    }
    private bool FindEqualSearchItemList(ItemSearchData items)
    {
        if (searchDatas.Count <= 0)
            return true;

        for (int i = 0; i < searchDatas.Count; i++)
        {
            if (searchDatas[i] == items)
                return false;
        }

        return true;
    }
    public void ClearSearchItem()
    {
        searchDatas.Clear();

    }

}
