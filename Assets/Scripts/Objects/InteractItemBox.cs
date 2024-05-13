using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
public class InteractItemBox : InteractObject
{
    public Action onUpdate;
    public enum ItemBoxState { Close, Opening, Open }
    [Networked, OnChangedRender(nameof(OnChangeState))] public ItemBoxState itemBoxState { get; set; } = ItemBoxState.Close;
    [SerializeField] private Transform openerTr;
    private float rotateValue;
    private float turnSpeed;
    private Coroutine processRoutine;

    [SerializeField] private ItemSpawnData[] spawnData;
    [Networked, Capacity(10), OnChangedRender(nameof(UpdateItem)), HideInInspector] public NetworkArray<Item> items { get; }
    [Networked] private string detectName { get; set; }
    protected override void Awake()
    {
        base.Awake();
        turnSpeed = 180f;
        rotateValue = -120f;
    }
    public override void Spawned()
    {

        base.Spawned();
        DetectData.interactHint = "아이템 상자 열기";



        if (HasStateAuthority)
        {
            itemBoxState = ItemBoxState.Close;
        }
        OnChangeState();


    }
    public override bool Detect(out DetectData interactInfo)
    {

        interactInfo = DetectData;


        return true;
    }


    public void ChangeState()
    {
        if (itemBoxState == ItemBoxState.Close)
        {
            if (HasStateAuthority)
            {
                itemBoxState = ItemBoxState.Opening;
            }
        }
        if (itemBoxState == ItemBoxState.Open)
        {
            playerInteract?.SearchItemInteract(this);
        }
    }
    private IEnumerator ProcessRoutin()
    {
        Quaternion targetQuat = Quaternion.Euler(rotateValue, 0f, 0f);
        float dotProduct = Quaternion.Dot(openerTr.rotation, targetQuat);

        while (Mathf.Abs(dotProduct) < 0.95f)
        {
            openerTr.rotation = Quaternion.RotateTowards(openerTr.rotation, Quaternion.Euler(rotateValue, 0f, 0f), turnSpeed * Time.deltaTime);

            dotProduct = Quaternion.Dot(openerTr.rotation, targetQuat);
            yield return null;
        }

        if (itemBoxState == ItemBoxState.Opening)
        {

            if (HasStateAuthority)
            {
                for (int i = 0; i < spawnData.Length; i++)
                {
                    Item itemInstance = Runner.Spawn(spawnData[i].SpawnItem);
                    items.Set(i, itemInstance);
                }
                itemBoxState = ItemBoxState.Open;
            }

        }

    }
    public override void StartInteraction()
    {
        ChangeState();
    }
    public void OnChangeState()
    {
        if (itemBoxState == ItemBoxState.Opening)
        {
            if (processRoutine == null)
                processRoutine = StartCoroutine(ProcessRoutin());
        }
        if (itemBoxState == ItemBoxState.Open)
        {
            DetectData.interactHint = "아이템 상자 탐색";
            openerTr.rotation = Quaternion.Euler(rotateValue, 0f, 0f);
        }
    }

    public override void OnExitDetect()
    {
        playerInteract?.StopSearchItemInteract();
        base.OnExitDetect();

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_AcquisitionItem(Inventory inventory, int index)
    {
        if (items[index] == null)
            return;

        inventory.AddItem(items[index]);
        items.Set(index, null);

    }
    public void UpdateItem()
    {
        onUpdate?.Invoke();
    }

}


[Serializable]
public class ItemSpawnData
{
    [SerializeField] private Item item;
    [SerializeField] public int probability;


    public Item SpawnItem { get { return item; } }

}