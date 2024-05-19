using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentSlotType { Helmet, Armor, Mask, Backpack, FirstMainWeapon, SecondMainWeapon, SubWeapon, MilyWeapon, Throw, Size }
public enum EquipmentType { Helmet, Mask, Armor, Backpack, Rifle, Pistol, Shotgun, Bazuka, SniperRifle, Mily, Size }

public class Equipment : NetworkBehaviour
{
    [SerializeField] private PlayerController owner;
    [SerializeField] private BodyPivotData[] ItemPivots;
    [Networked, HideInInspector, Capacity((int)EquipmentSlotType.Size), OnChangedRender(nameof(UpdateItemList))] public NetworkArray<EquipmentItem> netItems { get; }
    private EquipmentItem[] prevItems;
    private Inventory inventory;

    public event Action<int, Item> onItemUpdate;
    public event Action<int, Weapon> onMainWeaponUpdate;
    [SerializeField] private WeaponController weaponController;
    private void Awake()
    {
        prevItems = new EquipmentItem[(int)EquipmentSlotType.Size];
    }
    public void Init(Inventory inventory)
    {
        this.inventory = inventory;
    }

    private void EquipmentBodyItem(Item item)
    {
        if (item == null)
            return;

        item.SetParent(ItemPivots[(int)((EquipmentItemSO)item.ItemData).EquipmentType].mainPivot);
    }

    public Weapon GetWeapon(EquipmentSlotType equipmentSlotType)
    {
        if (equipmentSlotType < EquipmentSlotType.FirstMainWeapon)
            return null;

        return netItems[(int)equipmentSlotType] as Weapon;
    }
    public bool IsHaveWeapon(Weapon weapon)
    {
        Item findItem = Array.Find(netItems.ToArray(), (item) =>
         {
             if (item == null)
                 return false;

             return item == weapon;
         });


        if (findItem != null)
            return true;


        return false;

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
                netItems[index].UnEquip();
                netItems[index].RPC_SetActive(false);
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
                netItems[(int)slotTypes[i]].Equip(owner);
                item.RPC_SetActive(true);
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
        netItems[(int)slotTypes[0]].Equip(owner);
        item.RPC_SetActive(true);
        inventory.InsidePullItem(item);

    }


    private void UpdateItemList()
    {

        for (int i = 0; i < netItems.Length; i++)
        {



            if (netItems[i] != prevItems[i])
            {

                EquipmentBodyItem(netItems[i]);


                prevItems[i] = netItems[i];

            }


            onItemUpdate?.Invoke(i, netItems[i]);
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
