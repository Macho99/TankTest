using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemContainer : NetworkBehaviour,IAfterSpawned
{
    private ItemContainerUI itemContainerUI;
    [SerializeField] private PlayerInputListner InputListner;

    private bool isOpened;

    public ItemSearchSystem itemSearchSystem { get; set; }
    public Inventory inventory { get; set; }
    public Equipment equipment { get; set; }
    private void Awake()
    {
        itemSearchSystem = GetComponentInChildren<ItemSearchSystem>();
        inventory = GetComponentInChildren<Inventory>();
        equipment = GetComponentInChildren<Equipment>();
        itemSearchSystem.Init(inventory);
        equipment.Init(inventory);

    }
    private void Start()
    {

    }
    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            itemContainerUI = GameManager.UI.ShowPopUpUI<ItemContainerUI>("UI/PlayerUI/itemContainerUI",true);
            itemContainerUI.Init(this);
            itemContainerUI.CloseUI();

        }

        isOpened = false;
    }
    public override void FixedUpdateNetwork()
    {
        if (InputListner.pressButton.IsSet(ButtonType.ActiveItemContainer))
        {
            if (Runner.IsForward)
            {
                if (HasInputAuthority)
                {
                    if (!isOpened)
                    {
                        itemContainerUI = GameManager.UI.ShowPopUpUI<ItemContainerUI>("UI/PlayerUI/itemContainerUI");
                        isOpened = true;
                    }
                    else
                    {
                        itemContainerUI.CloseUI();
                        isOpened = false;
                    }
                }


            }
        }
    }
    public bool SetupSearchData(InteractItemBox searchItem)
    {

        if (!itemSearchSystem.AddSearchItem(searchItem))
            return false;

        return true;
    }
    public void RemoveSerachData()
    {
        itemSearchSystem.ClearSearchItem();
    }

    public void ActiveItemContainerUI(bool Active)
    {
        if (!HasInputAuthority)
            return;

        if (isOpened == Active)
            return;

        if (Active)
        {
            itemContainerUI = GameManager.UI.ShowPopUpUI<ItemContainerUI>("UI/PlayerUI/itemContainerUI");

            isOpened = true;
        }
        else
        {
            if (itemContainerUI != null)
                itemContainerUI.CloseUI();

            isOpened = false;
        }
    }
    public void AfterSpawned()
    {
       


    }
}
