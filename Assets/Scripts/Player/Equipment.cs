using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations.Rigging;



public enum WeaponAnimLayerType { Base, Pistol, Rifle, Shotgun, Bazuka, Mily, Size }
public enum EquipmentSlotType { Helmet, Armor, Mask, Backpack, FirstMainWeapon, SecondMainWeapon, SubWeapon, MilyWeapon, Throw, Size }
public enum EquipmentType { Helmet, Mask, Armor, Backpack, Rifle, Pistol, Shotgun, Bazuka, Mily, Size }
public class Equipment : NetworkBehaviour, IAfterSpawned
{
    [SerializeField] private PlayerController owner;
    [SerializeField] private Animator animator;
    [SerializeField] private EquipmentPivotData[] ItemPivots;
    [Networked, HideInInspector, Capacity((int)EquipmentSlotType.Size), OnChangedRender(nameof(UpdateItemList))] public NetworkArray<EquipmentItem> netItems { get; }
    private EquipmentItem[] localItems;
    [Header("WeaponRigs")]
    [SerializeField] private MultiAimConstraint handAimIK;
    [SerializeField] private TwoBoneIKConstraint subHandIK;
    [SerializeField] private PlayerAnimEvent animEvent;

    public event Action<int, Item> onItemUpdate;
    private Weapon mainWeapon { get; set; }
    [Networked, OnChangedRender(nameof(UpdateMainWeapon))] public Weapon NetMainWeapon { get; set; }

    private Inventory inventory;

    public Weapon GetMainWeapon() { return mainWeapon; }
    public void Init(Inventory inventory)
    {
        this.inventory = inventory;
    }
    private void Awake()
    {
        localItems = new EquipmentItem[(int)EquipmentSlotType.Size];
    }
    public override void Spawned()
    {

    }
    public override void FixedUpdateNetwork()
    {

    }
    public void SetEquipItem(EquipmentSlotType slotType, Item item)
    {




    }
    public void UpdateMainWeapon()
    {
        if (NetMainWeapon == null)
        {



            if (NetMainWeapon != null)
            {
                if (mainWeapon == null)
                {
                    mainWeapon = NetMainWeapon;
                    mainWeapon.Equip(owner);
                    //mainWeapon.SetTarget(subHandIK.data.target);
                    //animator.SetFloat("WeaponIndex", (float)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType);
                    //mainWeapon.SetParent(ItemPivots[(int)((EquipmentItemSO)mainWeapon.ItemData).EquipmentType].subPivot);
                    //animEvent.onStartDrawWeapon += OnStartDraw;
                    //animEvent.onEndDrawWeapon += OnEndDraw;
                    handAimIK.weight = 0f;
                    subHandIK.weight = 0f;
                    animator.SetTrigger("Draw");
                    Debug.Log("착용");
                }
            }


            if (mainWeapon != NetMainWeapon)
            {
                

            }
        }
    }
    public void OnStartDraw()
    {
        //mainWeapon.SetActive(true);
        //mainWeapon.SetParent(ItemPivots[(int)((EquipmentItemSO)mainWeapon.ItemData).EquipmentType].mainPivot);
        //Debug.Log("startDraw");


        //onItemUpdate?.Invoke((int)slotType, item);

    }
    private void EquipmentBodyItem(EquipmentSlotType slotType, EquipmentItem item)
    {
        if (localItems[(int)slotType] != null)
        {
            inventory.MoveItem(localItems[(int)slotType]);
            localItems[(int)slotType] = null;

        }

        item.SetParent(ItemPivots[(int)((EquipmentItemSO)item.ItemData).EquipmentType].mainPivot);
        item.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        item.RPC_SetActive(true);
        localItems[(int)slotType] = item;
    }
    private void SetupDrawRig(EquipmentType equipmentType)
    {
        animEvent.onStartDrawWeapon += OnStartDraw;
        animEvent.onEndDrawWeapon += OnEndDraw;
    }


    public void OnEndDraw()
    {
        
        
    }

  



    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Equipment(int index, Item item)
    {
        NetworkSetItem(index, item);

    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UnEquipment(int index, Item item)
    {
        inventory.MoveItem(netItems[index]);
        NetworkSetItem(index, null);
        NetworkSetItem(item);
    }




    public void NetworkSetItem(Item item)
    {
        if (item == null)
        {
            // netItems.Set()
        }

        if (item is EquipmentItem == false)
            return;

        //EquipmentSlotType[] slotTypes = ((EquipmentItemSO)item.ItemData).SlotTypes;

        //for (int i = 0; i < slotTypes.Length; i++)
        //{
        //    if (netItems[(int)slotTypes[i]] == null)
        //    {
        //        if (netItems[(int)slotTypes[i]] is Weapon)
        //        {
        //            if (NetMainWeapon == null)
        //            {
        //                NetMainWeapon = (Weapon)netItems[(int)slotTypes[i]];
        //            }
        //        }

        //        netItems.Set((int)slotTypes[i], (EquipmentItem)item);
        //        onItemUpdate?.Invoke((int)slotTypes[0], (EquipmentItem)item);
        //        return;
        //    }
        //}


        //inventory.InsideMoveItem(netItems[(int)slotTypes[0]]);
        //netItems.Set((int)slotTypes[0], (EquipmentItem)item);

        //onItemUpdate?.Invoke((int)slotTypes[0], (EquipmentItem)item);

    }
  
    public void NetworkSetItem(int index, Item item)
    {
        if (item == null)
        {
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
        netItems.Set((int)slotTypes[0], (EquipmentItem)item);
        inventory.InsidePullItem(index);


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
}

[Serializable]
public class EquipmentPivotData
{

    public EquipmentType Type;
    public Transform mainPivot;
    public Transform subPivot;
}


