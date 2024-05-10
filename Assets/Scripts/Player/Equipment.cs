using Fusion;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;



public class Equipment : NetworkBehaviour, IAfterSpawned
{
    public enum EquipmentSlotType { Helmet, Armor, Mask, Backpack, FirstMainWeapon, SecondMainWeapon, SubWeapon, MilyWeapon, Throw, Size }
    [SerializeField] private RuntimeAnimatorController baseAnimation;
    [SerializeField] private Animator animator;
    [SerializeField] private WeaponAnimData[] weaponAnims;
    [SerializeField] private EquipmentPivotData[] ItemPivots;

    [Networked, Capacity((int)EquipmentSlotType.Size), OnChangedRender(nameof(UpdateItem))] public NetworkArray<EquipmentItem> netItems { get; }
    [Header("WeaponRigs")]
    [SerializeField] private MultiAimConstraint handAimIK;
    [SerializeField] private TwoBoneIKConstraint subHandIK;
    private EquipmentItem[] localItems;
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private PlayerAnimEvent animEvent;
    private bool isConnectSubhand;
    private Weapon mainWeapon;
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
            if (localItems[(int)slotType] == mainWeapon)
            {

                //animator.runtimeAnimatorController = baseAnimation;

                mainWeapon = null;
            }
            localItems[(int)slotType] = null;
            return;
        }

        if ((item is EquipmentItem) == false)
        {
            Debug.Log("장착아이템이 아닙니다.");
            return;
        }

        if (item is Weapon)
        {
            if (mainWeapon == null)
            {
                //무기 장착시 무기 장착아이템이 존재하지않으면 무기왜폰할당
                mainWeapon = (Weapon)item;
                //무기종류에따라 애니메이션 변경
                if (mainWeapon is Rifle)
                {
                    isConnectSubhand = false;

                    item.SetParent(ItemPivots[(int)EquipmentPivotData.EquipmentType.Rifle].subPivot);
                    animator.runtimeAnimatorController = weaponAnims[(int)WeaponAnimData.WeaponAnimType.HeavyGun].animator;
                    animator.SetTrigger("Draw");
                    localItems[(int)EquipmentSlotType.FirstMainWeapon] = (EquipmentItem)item;
                    handAimIK.weight = 0f;
                    subHandIK.weight = 0f;

                    //집는모션 시작시

                    animEvent.onStartDrawWeapon += () =>
                    {
                        item.SetActive(true);
                        item.SetParent(ItemPivots[(int)EquipmentPivotData.EquipmentType.Rifle].mainPivot);
                        item.transform.localPosition = Vector3.zero;
                        item.transform.localRotation = Quaternion.identity;
                    };

                    animEvent.onEndDrawWeapon += () =>
                    {


                        handAimIK.weight = 1f;
                        subHandIK.weight = 1f;
                        isConnectSubhand = true;
                        Debug.Log(handAimIK.weight);
                        Debug.Log(subHandIK.weight);

                    };



                }

                EquipmentRevolver(item);
            }

        }
        else
        {

        }


    }
    private void LateUpdate()
    {
        if (mainWeapon != null)
        {
            if (isConnectSubhand)
            {
                Debug.Log(handAimIK.weight);
                Debug.Log(subHandIK.weight);
                subHandIK.data.target.SetPositionAndRotation(mainWeapon.SubHandTarget.position, mainWeapon.SubHandTarget.rotation);
            }
        }
    }
    private void EquipmentWeapon(Item item)
    {

    }
    private void EquipmentBackpack(Item item)
    {
        if (item is Backpack)
        {
            if (localItems[(int)EquipmentSlotType.Backpack] == null)
            {

            }
        }
    }
    private void EquipmentRevolver(Item item)
    {
        if (item is Revolver)
        {
            //item.SetParent(ItemPivots[(int)EquipmentPivotData.EquipmentType.Pistol].mainPivot);
            //animator.runtimeAnimatorController = weaponAnims[(int)WeaponAnimData.WeaponAnimType.LightGun].animator;
            //localItems[(int)EquipmentSlotType.FirstMainWeapon] = (EquipmentItem)item;
            //item.SetActive(true);
            ////아이템꺼내는 애니메이션 On
            //Debug.Log(mainWeapon.name);
            //TwoBoneIKConstraintData data = leftHandRig.data;
            //data.target = mainWeapon.SubHandTarget;
            //leftHandRig.data = data;
            //Debug.Log("착용" + item.name);
        }

    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Equipment(int index, Item item)
    {
        if (NetworkSetItem(item))
        {
            inventory.PullItem(index);
        }
    }



    public bool NetworkSetItem(Item item)
    {
        if (item is EquipmentItem == false)
            return false;

        EquipmentSlotType slotType = CheckSlottype(item);
        if (slotType == EquipmentSlotType.Size)
            return false;

        netItems.Set((int)slotType, (EquipmentItem)item);

        return true;

    }
    private void UpdateItem()
    {
        for (int i = 0; i < netItems.Length - 1; i++)
        {
            if (localItems[i] != netItems[i])
            {
                SetEquipItem((EquipmentSlotType)i, netItems[i]);
            }
        }
    }
    private EquipmentSlotType CheckSlottype(Item item)
    {
        if (item is Weapon)
        {
            WeaponType weaponType = ((WeaponItemSO)item.ItemData).GetWeaponType();

            if (weaponType == WeaponType.Main)
            {
                if (localItems[(int)EquipmentSlotType.FirstMainWeapon] == null)
                    return EquipmentSlotType.FirstMainWeapon;
                else if (localItems[(int)EquipmentSlotType.SecondMainWeapon] == null)
                    return EquipmentSlotType.SecondMainWeapon;
                else
                    return EquipmentSlotType.Size;
            }
            else if (weaponType == WeaponType.Sub)
            {
                if (localItems[(int)EquipmentSlotType.SubWeapon] == null)
                    return EquipmentSlotType.SubWeapon;
                else
                    return EquipmentSlotType.Size;
            }
            else if (weaponType == WeaponType.Mily)
            {
                if (localItems[(int)EquipmentSlotType.MilyWeapon] == null)
                    return EquipmentSlotType.MilyWeapon;
                else
                    return EquipmentSlotType.Size;
            }
            else if (weaponType == WeaponType.Throw)
            {
                if (localItems[(int)EquipmentSlotType.Throw] == null)
                    return EquipmentSlotType.Throw;
                else
                    return EquipmentSlotType.Size;
            }
        }
        else if (item is Armor)
        {

        }

        return EquipmentSlotType.Size;
    }

    public void AfterSpawned()
    {

        UpdateItem();
    }
}

[Serializable]
public class EquipmentPivotData
{
    public enum EquipmentType { Helmet, Mask, Armor, Backpack, Rifle, Pistol }
    public EquipmentType Type;
    public Transform mainPivot;
    public Transform subPivot;
}
[Serializable]
public class WeaponAnimData
{
    public enum WeaponAnimType
    {
        LightGun,
        HeavyGun,
        MliyWeapon
    }
    public WeaponAnimType Type;
    public RuntimeAnimatorController animator;

}