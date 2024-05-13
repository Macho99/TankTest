using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
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
    [Networked, HideInInspector, Capacity((int)EquipmentSlotType.Size), OnChangedRender(nameof(UpdateItem))] public NetworkArray<EquipmentItem> netItems { get; }
    private EquipmentItem[] localItems;
    [Header("WeaponRigs")]
    [SerializeField] private MultiAimConstraint handAimIK;
    [SerializeField] private TwoBoneIKConstraint subHandIK;
    [SerializeField] private PlayerAnimEvent animEvent;

    public event Action<int, Item> onItemUpdate;
    private Weapon mainWeapon { get; set; }
    [Networked, OnChangedRender(nameof(UpdateMainWeapon))] public Weapon NetMainWeapon { get; set; }
    private Inventory inventory;
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
    public void SetEquipItem(EquipmentSlotType slotType, Item item)
    {
        if (item == null)
        {
            if (localItems[(int)slotType] != null)
            {
                localItems[(int)slotType] = null;
                if (NetMainWeapon == item)
                {
                    NetMainWeapon = null;
                }
                onItemUpdate?.Invoke((int)slotType, null);
                return;
            }
        }



        //if (item == null)
        //{
        //    if (localItems[(int)slotType] != null)
        //    {
        //        inventory.InsideMoveItem(item);

        //        if (localItems[(int)slotType] == mainWeapon)
        //        {
        //            animator.SetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType, 0f);
        //            handAimIK.weight = 0f;
        //            subHandIK.weight = 0f;
        //            mainWeapon = null;
        //        }
        //    }
        //    localItems[(int)slotType].UnEquip();
        //    localItems[(int)slotType] = null;

        //    onItemUpdate?.Invoke((int)slotType, localItems[(int)slotType]);
        //    return;
        //}

        //if ((item is EquipmentItem) == false)
        //{
        //    Debug.Log("장착아이템이 아닙니다.");
        //    return;
        //}

        //if (item is Weapon)
        //{



        //    //if (localItems[(int)slotType] != null)
        //    //{
        //    //    if (item != localItems[(int)slotType])
        //    //    {
        //    //        if (NetMainWeapon == localItems[(int)slotType])
        //    //        {
        //    //            NetMainWeapon = null;
        //    //        }

        //    //        inventory.InsideMoveItem(localItems[(int)slotType]);

        //    //    }
        //    //}


        //    inventory.InsidePullItem(item);
        //    if (NetMainWeapon == null)
        //    {
        //        if (HasStateAuthority)
        //        {
        //            NetMainWeapon = (Weapon)item;
        //        }
        //    }
        //    localItems[(int)slotType] = (EquipmentItem)item;

        //}
        //else
        //{



        //}

        //onItemUpdate?.Invoke((int)slotType, localItems[(int)slotType]);
    }
    public void UpdateMainWeapon()
    {
        if (NetMainWeapon == null)
        {

        }
        if (NetMainWeapon != null)
        {
            if (mainWeapon == null)
            {
                mainWeapon = NetMainWeapon;
                mainWeapon.Equip(owner);
                mainWeapon.SetTarget(subHandIK.data.target);
                animator.SetFloat("WeaponIndex", (float)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType);
                mainWeapon.SetParent(ItemPivots[(int)((EquipmentItemSO)mainWeapon.ItemData).EquipmentType].subPivot);
                animEvent.onStartDrawWeapon += OnStartDraw;
                animEvent.onEndDrawWeapon += OnEndDraw;
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
    public void OnStartDraw()
    {
        mainWeapon.SetActive(true);
        mainWeapon.SetParent(ItemPivots[(int)((EquipmentItemSO)mainWeapon.ItemData).EquipmentType].mainPivot);
        Debug.Log("startDraw");

    }
    public void OnEndDraw()
    {
        Debug.Log("EndDraw");
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
                subHandIK.weight = currentValue;
                handAimIK.weight = currentValue;
            }

            yield return null;
        }
        animator.SetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType, 1f);
        if (mainWeapon.SubHandTarget != null)
        {
            subHandIK.weight = 1f;
            handAimIK.weight = 1f;
        }

    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Equipment(int index, Item item)
    {
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

        EquipmentSlotType[] slotTypes = ((EquipmentItemSO)item.ItemData).SlotTypes;

        for (int i = 0; i < slotTypes.Length; i++)
        {
            if (netItems[(int)slotTypes[i]] == null)
            {
                if (netItems[(int)slotTypes[i]] is Weapon)
                {
                    if (NetMainWeapon == null)
                    {
                        NetMainWeapon = (Weapon)netItems[(int)slotTypes[i]];
                    }
                }

                netItems.Set((int)slotTypes[i], (EquipmentItem)item);
                onItemUpdate?.Invoke((int)slotTypes[0], (EquipmentItem)item);
                return;
            }
        }


        inventory.InsideMoveItem(netItems[(int)slotTypes[0]]);
        netItems.Set((int)slotTypes[0], (EquipmentItem)item);

        onItemUpdate?.Invoke((int)slotTypes[0], (EquipmentItem)item);

    }
    private void UpdateItem()
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

        UpdateItem();
    }
}

[Serializable]
public class EquipmentPivotData
{

    public EquipmentType Type;
    public Transform mainPivot;
    public Transform subPivot;
}
