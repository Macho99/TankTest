using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public struct ItemStruct : INetworkStruct
{
    public NetworkBool isEmpty;
    public int itemID;
    public int count;

}
public class Inventory : NetworkBehaviour
{

    private List<ItemInstance> items = new List<ItemInstance>();
    [Networked, Capacity(100), OnChangedRender(nameof(ChangedItem))] NetworkArray<ItemStruct> netItems => default;

    [Networked] private int Weight { get; set; }
    [Networked] private float MaxWeight { get; set; }


    private void Awake()
    {

    }
    public void ChangedItem()
    {

        for (int i = 0; i < netItems.Length; i++)
        {
            //아이템이 삭제됬을떄
            if (netItems[i].isEmpty && items[i] != null)
            {
                items[i] = null;
            } //아이템이 추가된 상황
            else if (netItems[i].isEmpty == false && items[i] == null)
            {

            }
            // 아이템 정보가 바뀐상황
            if (netItems[i].itemID != items[i].ItemData.ItemID)
            {

            }
        }
    }

    public override void Spawned()
    {
        Weight = 0;
        MaxWeight = 100f;
        items = new List<ItemInstance>();


    }

    public void AddItem(ItemInstance newItem)
    {


    }
    public void RemoveItem(int index)
    {
        ItemStruct itemStruct = new ItemStruct();
        itemStruct.isEmpty = true;
        itemStruct.itemID = default;
        itemStruct.count = default;
        netItems.Set(0, itemStruct);
    }


    private void Checkweight(Item item)
    {
        //float itemWeight = item.ItemData.Weight;


        //if (itemWeight + this.Weight > MaxWeight)
        //{

        //}

    }
}
