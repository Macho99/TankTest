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
    public void SetupSearchData(ItemSearchData searchData)
    {

        if (!itemSearchSystem.AddSearchItem(searchData))
            return;
    }
    public void RemoveSerachData(ItemSearchData searchData)
    {
        itemSearchSystem.ClearsearchItem(searchData);
    }
    public void ActiveItemContainerUI(bool Active)
    {
        if (!HasInputAuthority)
            return;

        if (isOpened == Active)
            return;

        Debug.Log(Active);
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
