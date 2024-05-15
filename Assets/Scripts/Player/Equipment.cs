using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations.Rigging;




public enum EquipmentSlotType { Helmet, Armor, Mask, Backpack, FirstMainWeapon, SecondMainWeapon, SubWeapon, MilyWeapon, Throw, Size }
public enum EquipmentType { Helmet, Mask, Armor, Backpack, Rifle, Pistol, Shotgun, Bazuka, Mily, Size }
public class Equipment : NetworkBehaviour, IAfterSpawned
{
    [SerializeField] private PlayerController owner;
    [SerializeField] private BodyPivotData[] ItemPivots;
    [Networked, HideInInspector, Capacity((int)EquipmentSlotType.Size), OnChangedRender(nameof(UpdateItemList))] public NetworkArray<EquipmentItem> netItems { get; }
    private EquipmentItem[] localItems;
    private Inventory inventory;

    public event Action<int, Item> onItemUpdate;
    public event Action<int, Weapon> onMainWeaponUpdate;
    [SerializeField] private WeaponController weaponController;
    public void Init(Inventory inventory)
    {
        this.inventory = inventory;
    }
    private void Awake()
    {
        localItems = new EquipmentItem[(int)EquipmentSlotType.Size];
    }



    public void SetEquipItem(EquipmentSlotType slotType, Item item)
    {

        if (item == null)
        {
            if (localItems[(int)slotType] != null)
            {
                if (localItems[(int)slotType] is Weapon == false)
                {
                    localItems[(int)slotType].UnEquip();
                    localItems[(int)slotType].SetActive(false);
                }
            }
        }
        else
        {

            if (localItems[(int)slotType] != null)
            {
                localItems[(int)slotType].UnEquip();
                localItems[(int)slotType].SetActive(false);

            }

            EquipmentBodyItem(item);

        }




        if (item != null)
        {
            localItems[(int)slotType] = (EquipmentItem)item;
        }
        else
            localItems[(int)slotType] = null;




        onItemUpdate?.Invoke((int)slotType, localItems[(int)slotType]);
    }

    private void EquipmentBodyItem(Item item)
    {
        ((EquipmentItem)item).Equip(owner);
        item.SetParent(ItemPivots[(int)((EquipmentItemSO)item.ItemData).EquipmentType].mainPivot);
        item.SetActive(true);
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Equipment(int index, Item item)
    {
        NetworkSetItem(index, item);

    }
    public void NetworkSetItem(int index, Item item)
    {
        if (item == null)
        {
            if (netItems[index] != null)
            {
                if (netItems[index] is Weapon)
                {
                    onMainWeaponUpdate?.Invoke(index, (Weapon)item);
                }
                inventory.InsideMoveItem(netItems[index]);
            }
            netItems.Set(index, null);
            return;
        }

        if (item is EquipmentItem == false)
            return;


        EquipmentSlotType[] slotTypes = ((EquipmentItemSO)item.ItemData).SlotTypes;
        for (int i = 0; i < slotTypes.Length; i++)
        {
            if (netItems[(int)slotTypes[i]] == null)
            {
                netItems.Set((int)slotTypes[i], (EquipmentItem)item);
                inventory.InsidePullItem(index);
                return;
            }
        }
        //아이템이 둘다 꽉차있어서 체인지하는상황


        Item changeItem = netItems[(int)slotTypes[0]];
        inventory.InsideMoveItem(changeItem);
        if (item is Weapon)
        {
            onMainWeaponUpdate?.Invoke((int)slotTypes[0], (Weapon)item);
        }
        netItems.Set((int)slotTypes[0], (EquipmentItem)item);

        inventory.InsidePullItem(item);

    }


    private void UpdateItemList()
    {
        for (int i = 0; i < netItems.Length - 1; i++)
        {
            if (localItems[i] != netItems[i])
            {
                //아이템뺴는상황// 아이템이 스왑하는상황 // 아이템 착용하는상황
                SetEquipItem((EquipmentSlotType)i, netItems[i]);
            }

        }
    }
    public void AfterSpawned()
    {
        UpdateItemList();
    }

    public bool IsExistEquipmentItem(EquipmentItem item)
    {
        return Array.Find(netItems.ToArray(), (slotItem) => { return item == slotItem; });
    }
}

[Serializable]
public class BodyPivotData
{

    public EquipmentType Type;
    public Transform mainPivot;


}


