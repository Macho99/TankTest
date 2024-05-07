using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemContainerUI : PopUpUI
{
    [SerializeField] private ItemSearchUI searchUI;

    private ItemContainer itemContainer;
    public void Init(ItemContainer itemContainer)
    {
        this.itemContainer = itemContainer;
        searchUI.Init(itemContainer.itemSearchSystem, itemContainer.inventory);


    }

    public void PerformAddSearchItem(List<ItemInstance> items)
    {
        // searchUI.AddSearchItem(items);
        //등록하면서 이벤트들을 등록해야됨

        //오른쪽클릭시 인벤토리로 아이템 이동
        //왼쪽버튼으로 드래그시 이동
    }

    public void ClearSearchItemSlotUI()
    {

    }
}
