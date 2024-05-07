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
        //����ϸ鼭 �̺�Ʈ���� ����ؾߵ�

        //������Ŭ���� �κ��丮�� ������ �̵�
        //���ʹ�ư���� �巡�׽� �̵�
    }

    public void ClearSearchItemSlotUI()
    {

    }
}
