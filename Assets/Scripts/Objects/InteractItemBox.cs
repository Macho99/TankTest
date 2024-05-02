using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractItemBox : InteractObject
{

    public enum ItemBoxState { Open = -1, Close = 1 }
    [Networked] public ItemBoxState itemBoxState { get; set; }
    [SerializeField] private Transform openerTr;
    private float rotateValue;
    private float turnSpeed;
    private Coroutine processRoutine;
    private void Awake()
    {
        turnSpeed = 1f;
        rotateValue = -120f;
    }
    public override void Spawned()
    {
        base.Spawned();
        if (HasStateAuthority)
        {
            itemBoxState = ItemBoxState.Close;
        }

        if (itemBoxState == ItemBoxState.Close)
        {
            interactData.interactHint = "아이템 상자 열기";
        }
        else
        {
            interactData.interactHint = "아이템 상자 탐색";
        }

        //openerTr.rotation = Quaternion.Euler(-120, 0f, 0f);
    }

    public override bool Detect(out InteractData interactInfo)
    {

        interactInfo = interactData;

        return true;
    }
    public override bool Interact(out InteractObject interactObject)
    {
        interactObject = this;


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

        }
    }
    private IEnumerator ProcessRoutin()
    {

        Quaternion targetQuat = Quaternion.Euler(rotateValue, 0f, 0f);
        float dotProduct = Quaternion.Dot(openerTr.rotation, targetQuat);

        while (Mathf.Abs(dotProduct) < 0.99f)
        {
            openerTr.rotation = Quaternion.Slerp(openerTr.rotation, Quaternion.Euler(rotateValue, 0f, 0f), turnSpeed * Time.deltaTime);
            yield return null;
        }

    }

}
