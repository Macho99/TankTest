using Fusion;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public enum EquipmentType { Helmet, Mask, Armor, Backpack, Rifle, Shotgun, Bazuka, Pistol, Mily }
public enum WeaponAnimLayer { Base, Pistol, Riple, Shotgun, Bazuka, Mily, Size }
public enum EquipmentSlotType { Helmet, Armor, Mask, Backpack, FirstMainWeapon, SecondMainWeapon, SubWeapon, MilyWeapon, Throw, Size }

public enum WeaponSwichingState { None, Changing }

public class Equipment : NetworkBehaviour, IAfterSpawned
{

    [SerializeField] private RuntimeAnimatorController baseAnimation;
    [SerializeField] private Animator animator;
    [SerializeField] private EquipmentPivotData[] ItemPivots;
    [Networked, Capacity((int)EquipmentSlotType.Size), OnChangedRender(nameof(UpdateItemList))] public NetworkArray<EquipmentItem> netItems { get; }

    [Networked] public NetworkBool isChanged { get; private set; }
    private EquipmentItem[] localItems;
    [Header("WeaponRigs")]
    [SerializeField] private Rig handAimRigLayer;
    [SerializeField] private TwoBoneIKConstraint subHandIK;
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private PlayerAnimEvent animEvent;
    [SerializeField] private PlayerController owner;
    [Networked] private Weapon mainWeapon { get; set; }
    private Inventory inventory;

    [Networked] public WeaponSwichingState swichingState { get; private set; }
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
        Debug.Log(swichingState);
    }
    public void SetEquipItem(EquipmentSlotType slotType, Item item)
    {
        if (item == null)
        {

            if (localItems[(int)slotType] != null)
            {
                if (localItems[(int)slotType] == mainWeapon)
                {

                    StopAllCoroutines();
                    subHandIK.weight = 0f;
                    handAimRigLayer.weight = 0f;
                    animEvent.onStartDrawWeapon -= OnStartDraw;
                    animEvent.onEndDrawWeapon -= OnEndDraw;
                    animEvent.onEndPutWeapon -= OnEndPut;
                    if (animator.GetCurrentAnimatorStateInfo((int)WeaponAnimLayer.Size).IsName("Empty"))
                        animator.ResetTrigger("Cancle");
                    else
                    {
                        animator.SetTrigger("Cancle");
                    }

                    Debug.Log(((WeaponItemSO)mainWeapon.ItemData).GetWeaponAnimLayer());
                    animator.SetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).GetWeaponAnimLayer(), 0f);
                    mainWeapon.UnEquip();
                    mainWeapon = null;
                }
                localItems[(int)slotType] = (EquipmentItem)item;
            }
            localItems[(int)slotType] = null;
            onItemUpdate?.Invoke((int)slotType, null);
            Debug.Log("Unequip");
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
                swichingState = WeaponSwichingState.Changing;
                mainWeapon = (Weapon)item;
                mainWeapon.Equip(owner);
                animator.SetFloat("WeaponIndex", (float)((WeaponItemSO)mainWeapon.ItemData).GetWeaponAnimLayer());
                handAimRigLayer.weight = 0f;
                subHandIK.weight = 0f;
                animator.SetTrigger("Draw");
                item.SetParent(ItemPivots[(int)((EquipmentItemSO)mainWeapon.ItemData).EquipmentType].subPivot);
                SetupDrawRig(((EquipmentItemSO)mainWeapon.ItemData).EquipmentType);

            }


            localItems[(int)slotType] = (EquipmentItem)item;
        }
        else
        {
            EquipmentBodyItem(slotType, (EquipmentItem)item);

        }



        onItemUpdate?.Invoke((int)slotType, item);

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

    #region AnimEvent

    private void OnStartDraw()
    {
        mainWeapon.RPC_SetActive(true);
        mainWeapon.SetParent(ItemPivots[(int)((EquipmentItemSO)mainWeapon.ItemData).EquipmentType].mainPivot);
        mainWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
    private void OnEndDraw()
    {
        if (mainWeapon is Weapon)
            StartCoroutine(DrawWeaponRoutine());
    }
    private void OnEndPut()
    {
        mainWeapon.SetParent(ItemPivots[(int)((EquipmentItemSO)mainWeapon.ItemData).EquipmentType].subPivot);
        mainWeapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        mainWeapon.UnEquip();
        mainWeapon = null;
        swichingState = WeaponSwichingState.None;
    }
    private IEnumerator DrawWeaponRoutine()
    {
        float speed = 2f;


        float weight = 0f;
        while (animator.GetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).GetWeaponAnimLayer()) < 1f)
        {
            weight += speed * Time.deltaTime;
            subHandIK.weight = weight;
            handAimRigLayer.weight = weight;

            animator.SetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).GetWeaponAnimLayer(), weight);
            yield return null;
        }

        subHandIK.weight = 1f;
        handAimRigLayer.weight = 1f;

        animator.SetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).GetWeaponAnimLayer(), 1f);
        subHandIK.data.target = mainWeapon.SubHandTarget;
        rigBuilder.Build();
        swichingState = WeaponSwichingState.None;
    }
    #endregion



    #region RPCMethod

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

    }
    #endregion

    public void TakeOnMainWeapon(EquipmentSlotType slotType)
    {
        if (localItems[(int)slotType] == null)
            return;

        if (swichingState == WeaponSwichingState.Changing)
            return;

        if (mainWeapon == localItems[(int)slotType])
        {
            return;
        }


        swichingState = WeaponSwichingState.Changing;
        subHandIK.weight = 0f;
        handAimRigLayer.weight = 0f;

        if (mainWeapon != null)
        {
            StopAllCoroutines();
            if (animator.GetCurrentAnimatorStateInfo((int)WeaponAnimLayer.Size).IsName("Empty"))
                animator.ResetTrigger("Cancle");
            else
            {
                animator.SetTrigger("Cancle");
            }

            animEvent.onStartDrawWeapon -= OnStartDraw;
            animEvent.onEndDrawWeapon -= OnEndDraw;
            animEvent.onEndPutWeapon -= OnEndPut;


            animator.SetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).GetWeaponAnimLayer(), 0f);
            mainWeapon.SetParent(ItemPivots[(int)((EquipmentItemSO)mainWeapon.ItemData).EquipmentType].subPivot);

            mainWeapon.UnEquip();
            mainWeapon = (Weapon)netItems[(int)slotType];
            mainWeapon.Equip(owner);

            netItems.Set((int)slotType, mainWeapon);
            animator.SetFloat("WeaponIndex", (float)((WeaponItemSO)mainWeapon.ItemData).GetWeaponAnimLayer());
            mainWeapon.SetParent(ItemPivots[(int)((EquipmentItemSO)mainWeapon.ItemData).EquipmentType].subPivot);
            animator.SetTrigger("Draw");
            SetupDrawRig(((EquipmentItemSO)mainWeapon.ItemData).EquipmentType);

            return;
        }



        mainWeapon = (Weapon)localItems[(int)slotType];
        mainWeapon.Equip(owner);
        animator.SetFloat("WeaponIndex", (float)((WeaponItemSO)mainWeapon.ItemData).GetWeaponAnimLayer());
        mainWeapon.SetParent(ItemPivots[(int)((EquipmentItemSO)mainWeapon.ItemData).EquipmentType].subPivot);
        animator.SetTrigger("Draw");
        SetupDrawRig(((EquipmentItemSO)mainWeapon.ItemData).EquipmentType);
        //mainWeapon = (Weapon)localItems[(int)slotType];




    }
    public void TakeOffMainWeapon()
    {
        swichingState = WeaponSwichingState.Changing;

        subHandIK.weight = 0f;
        handAimRigLayer.weight = 0f;
        animator.SetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).GetWeaponAnimLayer(), 0f);
        animEvent.onEndPutWeapon += OnEndPut;
        animator.SetTrigger("Put");
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


        EquipmentSlotType[] slotTypes = ((EquipmentItemSO)item.ItemData).SlotType;
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


