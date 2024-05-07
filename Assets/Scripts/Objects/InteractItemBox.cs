using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using Random = UnityEngine.Random;
public class InteractItemBox : InteractObject, IInterestEnter, IInterestExit
{

    public enum ItemBoxState { Open = -1, Close = 1 }
    [Networked, OnChangedRender(nameof(OnChangeState))] public ItemBoxState itemBoxState { get; set; }
    [SerializeField] private Transform openerTr;
    private float rotateValue;
    private float turnSpeed;
    private Coroutine processRoutine;
    [SerializeField] private List<ItemSO> itemDataList;
    private ItemSearchData searchData;
    [Networked, Capacity(20)] private NetworkArray<Item> items { get; }
    [Networked] private string detectName { get; set; }
    protected override void Awake()
    {
        base.Awake();
        turnSpeed = 1f;
        rotateValue = -120f;
    }
    public override void Spawned()
    {
        searchData = new ItemSearchData(this);
        for (int i = 0; i < itemDataList.Count; i++)
        {
            int randCount = 1;
            if (itemDataList[i].IsStackable)
            {
                randCount = Random.Range(1, itemDataList[i].MaxCount);
            }
            searchData.AddItemData(itemDataList[i].CreateItemData(randCount));

        }



        base.Spawned();
        DetectData.interactHint = "아이템 상자 열기";
        if (HasStateAuthority)
        {
            itemBoxState = ItemBoxState.Close;

            //int newIndex = 0;
            //for (int i = 0; i < itemDataList.Count; i++)
            //{

            //    ItemInstance instance = itemDataList[i].CreateItemData();
            //    items.Set(newIndex, (instance.CreateNetworkItem(Runner)));
            //    items[newIndex].gameObject.SetActive(false);
            //    newIndex++;

            //}
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
            playerInteract?.SearchItemInteract(searchData);
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
        playerInteract?.StopSearchItemInteract(searchData);
        base.OnExitDetect();

    }
    public void DeleteItem()
    {

    }

    public void InterestEnter(PlayerRef player)
    {
        Debug.Log("InterestEnter : " + player);
    }

    public void InterestExit(PlayerRef player)
    {
        Debug.Log("InterestExit : " + player);
    }
}
[Serializable]
public class ItemSearchData
{
    private NetworkBehaviour owner;
    public List<ItemInstance> itemList;
    public Action<int> onItemAdd;
    public Action<int> onItemDelete;
    public ItemSearchData(NetworkBehaviour owner)
    {
        this.owner = owner;
        itemList = new List<ItemInstance>();
    }

    public ItemSearchData(NetworkBehaviour owner, List<ItemInstance> itemList)
    {
        this.owner = owner;
        this.itemList = itemList;
    }

    public void AddItemData(ItemInstance item)
    {
        itemList.Add(item);
    }
    public void CreateItem(NetworkRunner runner, int index)
    {
        if (owner.HasStateAuthority)
        {
            itemList[index].CreateNetworkItem(runner);
        }
        itemList.RemoveAt(index);
    }
}