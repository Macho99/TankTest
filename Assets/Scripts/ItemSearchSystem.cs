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
        if (FindEqualSearchItemList(items, out int listIndex))
        {
            Debug.Log("Ã£À½");
            return false;
        }

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
    private bool FindEqualSearchItemList(ItemSearchData items, out int index)
    {
        for (int i = 0; i < searchDatas.Count; i++)
        {
            if (searchDatas[i] == items)
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
    }

    public void ClearsearchItem(ItemSearchData items)
    {
        if (FindEqualSearchItemList(items, out int index))
        {
            for (int j = 0; j < searchDatas[index].itemList.Count; j++)
            {
                onUpdate?.Invoke(j, null);
            }
            searchDatas.RemoveAt(index);
        }


    }
    public void AllClearSearchItem()
    {
        searchDatas.Clear();
    }

    public void AcquisitionItem(int index)
    {
       
    }

}
