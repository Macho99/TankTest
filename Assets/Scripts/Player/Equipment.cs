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
    private Inventory inventory;

    [Networked, HideInInspector, OnChangedRender(nameof(UpdateMainWeapon))] public Weapon NetMainWeapon { get; set; }
    private Weapon mainWeapon { get; set; }

    public event Action<int, Item> onItemUpdate;

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



        localItems[(int)slotType] = (EquipmentItem)item;



        onItemUpdate?.Invoke((int)slotType, localItems[(int)slotType]);
    }
    public void UpdateMainWeapon()
    {
        Debug.Log("update");

        if (NetMainWeapon == null)
        {
            Debug.Log("널");
        }
        else
        {
            if (mainWeapon == null)
            {
                mainWeapon = NetMainWeapon;
                Debug.Log(mainWeapon.name);
                SetupMainWeapon();
            }
            else
            {
                Debug.Log("asdasdsad");
                handAimIK.weight = 0f;
                subHandIK.weight = 0f;
                mainWeapon.UnEquip();
                mainWeapon.SetTarget(null);
                mainWeapon.SetActive(false);

                if (HasStateAuthority)
                    animator.SetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType, 0f);
                SetupMainWeapon();

            }
        }

    }
    private void SetupMainWeapon()
    {
        mainWeapon = NetMainWeapon;
        mainWeapon.Equip(owner);
        animator.SetTrigger("Draw");
        mainWeapon.SetTarget(subHandIK.data.target);
        animator.SetFloat("WeaponIndex", (float)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType);
        mainWeapon.SetParent(ItemPivots[(int)((EquipmentItemSO)mainWeapon.ItemData).EquipmentType].subPivot);
        animEvent.onStartDrawWeapon += OnStartDraw;
        animEvent.onEndDrawWeapon += OnEndDraw;
        handAimIK.weight = 0f;
        subHandIK.weight = 0f;
    }
    public void OnStartDraw()
    {
        mainWeapon.SetActive(true);
        mainWeapon.SetParent(ItemPivots[(int)((EquipmentItemSO)mainWeapon.ItemData).EquipmentType].mainPivot);

        Debug.Log("Start");
    }
    public void OnEndDraw()
    {
        StartCoroutine(DrawRoutine());

    }
    private IEnumerator DrawRoutine()
    {
        float speed = 2f;
        float currentValue = 0f;

        while (animator.GetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType) < 1f)
        {
            currentValue += speed * Time.deltaTime;
            animator.SetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType, currentValue);

            if (mainWeapon.SubHandTarget != null)
            {
                handAimIK.weight += currentValue;
                subHandIK.weight += currentValue;
            }


            yield return null;
        }
        if (mainWeapon.SubHandTarget != null)
        {
            handAimIK.weight += 1f;
            subHandIK.weight += 1f;
        }
        animator.SetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType, 1f);

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
                if (netItems[index] == NetMainWeapon)
                {
                    NetMainWeapon = null;
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
                if (NetMainWeapon == null)
                {
                    NetMainWeapon = (Weapon)netItems[(int)slotTypes[i]];
                }

                inventory.InsidePullItem(index);
                return;
            }
        }

        if (netItems[(int)slotTypes[0]] == NetMainWeapon)
        {
            NetMainWeapon = (Weapon)item;
        }

        Item changeItem = netItems[(int)slotTypes[0]];
        inventory.InsideMoveItem(changeItem);
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
        UpdateMainWeapon();
    }
}

[Serializable]
public class EquipmentPivotData
{

    public EquipmentType Type;
    public Transform mainPivot;
    public Transform subPivot;
}


