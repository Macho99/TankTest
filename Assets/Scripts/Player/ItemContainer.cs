using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemContainer : NetworkBehaviour
{
    private ItemContainerUI itemContainerUI;
    [SerializeField] private PlayerInputListner InputListner;

    private bool isOpened;

    public ItemSearchSystem itemSearchSystem { get; set; }
    public Inventory inventory { get; set; }
    private void Awake()
    {
        itemSearchSystem = GetComponentInChildren<ItemSearchSystem>();
        inventory = GetComponentInChildren<Inventory>();
        itemSearchSystem.Init(inventory);

    }
    private void Start()
    {

    }
    public override void Spawned()
    {

        if (HasInputAuthority)
        {
            itemContainerUI = GameManager.UI.ShowPopUpUI<ItemContainerUI>("UI/PlayerUI/itemContainerUI");
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
    public void SetupSearchData(NetworkArray<Item> searchItem)
    {

        if (!itemSearchSystem.AddSearchItem(searchItem))
            return;
    }
    public void RemoveSerachData(NetworkArray<Item> searchItem)
    {
        //itemSearchSystem.ClearsearchItem(searchItem);
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
    public void ExitItemSearch()
    {

    }


}
