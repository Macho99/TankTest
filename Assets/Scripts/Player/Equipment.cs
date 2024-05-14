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
using static UnityEditor.Progress;



public enum WeaponSwichingState { None, Changing }
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

    [Networked] public WeaponSwichingState swichingState { get; private set; }

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


    public void EquipMainWeapon(EquipmentSlotType slotType)
    {
        if (netItems[(int)slotType] == null)
            return;

        NetMainWeapon = (Weapon)netItems[(int)slotType];
    }
    public void UnEquipMainWeapon()
    {
        if (NetMainWeapon != null)
        {
            NetMainWeapon = null;
        }
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
            if (item is Weapon == false)
            {
                if (localItems[(int)slotType] != null)
                {
                    localItems[(int)slotType].UnEquip();
                    localItems[(int)slotType].SetActive(false);

                }

                EquipmentBodyItem(item);


            }
        }




        if (item != null)
        {
            if (item is Weapon)
            {
                if (NetMainWeapon != localItems[(int)slotType])
                {
                    item.SetActive(true);
                    item.SetParent(ItemPivots[(int)((EquipmentItemSO)item.ItemData).EquipmentType].subPivot);
                }
            }
            localItems[(int)slotType] = (EquipmentItem)item;
        }
        else
            localItems[(int)slotType] = null;




        onItemUpdate?.Invoke((int)slotType, localItems[(int)slotType]);
    }

    private void EquipmentBodyItem(Item item)
    {
        ((EquipmentItem)item).Equip(owner);
        item.SetActive(true);
        item.SetParent(ItemPivots[(int)((EquipmentItemSO)item.ItemData).EquipmentType].mainPivot);
    }
    public void UpdateMainWeapon()
    {
        Debug.Log("update");

        if (NetMainWeapon == null)
        {
            if (mainWeapon != null)
            {
                StopAllCoroutines();
                handAimIK.weight = 0f;
                subHandIK.weight = 0f;
                mainWeapon.SetTarget(null);
                EquipmentItem equipmentItem = Array.Find(localItems.ToArray(), (item) => { return mainWeapon == item; });
                if (equipmentItem == null)
                {
                    animator.SetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType, 0f);
                    mainWeapon = null;
                }
                else
                {
                    swichingState = WeaponSwichingState.Changing;
                    animEvent.onStartPutWeapon += OnStartPut;
                    animator.SetTrigger("Put");
                    Debug.Log("메인무기아이템이 장비창에존재");
                }





            }

        }
        else
        {
            swichingState = WeaponSwichingState.Changing;
            if (mainWeapon == null)
            {
                mainWeapon = NetMainWeapon;
                SetupMainWeapon();
            }
            else
            {
                StopAllCoroutines();
                animEvent.onStartDrawWeapon -= OnStartDraw;
                animEvent.onEndDrawWeapon -= OnEndDraw;
                handAimIK.weight = 0f;
                subHandIK.weight = 0f;
                mainWeapon.UnEquip();
                mainWeapon.SetTarget(null);
                mainWeapon.SetActive(false);


                animator.SetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType, 0f);
                Debug.Log((int)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType);
                Debug.Log(animator.GetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType));
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
    public void OnStartPut()
    {

        mainWeapon.SetParent(ItemPivots[(int)((EquipmentItemSO)mainWeapon.ItemData).EquipmentType].subPivot);
        animator.SetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType, 0f);
        mainWeapon = null;
        swichingState = WeaponSwichingState.None;
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
        swichingState = WeaponSwichingState.None;
    }
    public void ChangeWeaponWeight(float weight)
    {
        if (mainWeapon == null)
            return;

        handAimIK.weight = weight;
        subHandIK.weight = weight;
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

                if (item is Weapon)
                {
                    if (NetMainWeapon == null)
                    {
                        NetMainWeapon = (Weapon)netItems[(int)slotTypes[i]];
                    }

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


