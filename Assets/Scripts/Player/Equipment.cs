using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;



public class Equipment : NetworkBehaviour
{
    public enum EquipmentSlotType { Helmet, Armor, Mask, Backpack, MainWeapon, SubWeapon, MilyWeapon, }
    [SerializeField] private Animator animator;
    [SerializeField] private WeaponAnimData[] weaponAnims;
    [SerializeField] private EquipmentPivotData[] ItemPivots;
    [Header("WeaponRigs")]

    [SerializeField] private TwoBoneIKConstraint leftHandRig;
    private EquipmentItem[] EquipmentItems;

    private Weapon mainWeapon;
    private Inventory inventory;
    public void Init(Inventory inventory)
    {
        this.inventory = inventory;
    }
    private void Awake()
    {
        EquipmentItems = new EquipmentItem[ItemPivots.Length];

    }
    public void SetEquipItem(Item item)
    {
        Debug.Log(item.name);
        if ((item is EquipmentItem) == false)
        {
            Debug.Log("������������ �ƴմϴ�.");
            return;
        }

        if (item is Weapon)
        {
            if (mainWeapon == null)
            {
                //���� ������ ���� ������������ �������������� ��������Ҵ�
                mainWeapon = (Weapon)item;
                //�������������� �ִϸ��̼� ����
                if (mainWeapon is Rifle)
                {

                    item.SetParent(ItemPivots[(int)EquipmentPivotData.EquipmentType.Rifle].mainPivot);
                    animator.runtimeAnimatorController = weaponAnims[(int)WeaponAnimData.WeaponAnimType.LightGun].animator;
                    EquipmentItems[(int)EquipmentSlotType.MainWeapon] = (EquipmentItem)item;
                    item.SetActive(true);
                    //�����۲����� �ִϸ��̼� On
                    Debug.Log(mainWeapon.name);
                    TwoBoneIKConstraintData data = leftHandRig.data;
                    data.target = mainWeapon.SubHandPivot;
                    leftHandRig.data = data;
                    Debug.Log("����" + item.name);
                }
            }

        }
        else
        {

        }


    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_Equipment(int index)
    {
        Debug.Log(index);
        Item item = inventory.PullItem(index);        
        SetEquipItem(item);
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