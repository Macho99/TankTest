using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
public class InteractItemBox : InteractObject
{

    public enum ItemBoxState { Open = -1, Close = 1 }
    [Networked, OnChangedRender(nameof(OnChangeState))] public ItemBoxState itemBoxState { get; set; }
    [SerializeField] private Transform openerTr;
    private float rotateValue;
    private float turnSpeed;
    private Coroutine processRoutine;
    [SerializeField] private List<ItemSO> itemDataList;
    [SerializeField] private ItemSpawnData[] spawnData;

    [Networked, Capacity(10)] private NetworkArray<Item> items { get; }
    [Networked] private string detectName { get; set; }
    protected override void Awake()
    {
        base.Awake();
        turnSpeed = 1f;
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
            if (processRoutine == null)
                processRoutine = StartCoroutine(ProcessRoutin());
        }
        else
        {

            playerInteract?.SearchItemInteract(items);
        }
    }
    private IEnumerator ProcessRoutin()
    {

        Quaternion targetQuat = Quaternion.Euler(rotateValue, 0f, 0f);
        float dotProduct = Quaternion.Dot(openerTr.rotation, targetQuat);

        while (Mathf.Abs(dotProduct) < 0.95f)
        {
            openerTr.rotation = Quaternion.Slerp(openerTr.rotation, Quaternion.Euler(rotateValue, 0f, 0f), turnSpeed * Time.deltaTime);

            dotProduct = Quaternion.Dot(openerTr.rotation, targetQuat);
            yield return null;
        }

        if (itemBoxState == ItemBoxState.Close)
        {
            itemBoxState = ItemBoxState.Open;

            if (HasStateAuthority)
            {
                for (int i = 0; i < spawnData.Length; i++)
                {
                    Item itemInstance = Runner.Spawn(spawnData[i].SpawnItem);
                    items.Set(i, itemInstance);

                }
            }
        }

    }
    public override void StartInteraction()
    {

        ChangeState();
    }
    public void OnChangeState()
    {
        if (itemBoxState == ItemBoxState.Open)
        {
            DetectData.interactHint = "아이템 상자 탐색";
        }
    }

    public override void OnExitDetect()
    {
        //playerInteract?.StopSearchItemInteract(localSearchData);
        base.OnExitDetect();

    }


}
public struct ItemSearchData : INetworkStruct
{
    public NetworkId ownerId;
    public NetworkBehaviour owner;
    public List<ItemInstance> itemList;
    public event Action<int> onRemove;
    public Action<ItemSearchData> onUpdate;

}
[Serializable]
public class ItemSpawnData
{
    [SerializeField] private Item item;
    [SerializeField] public int probability;


    public Item SpawnItem { get { return item; } }

}