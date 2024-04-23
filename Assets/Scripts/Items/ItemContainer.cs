using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemContainer : InteractObject
{
    public enum ContainerState { Open, Close }
    private Item[] itemArray;
    [SerializeField] private float openAngleValue;
    private float closeAngleValue;
    [SerializeField] private Transform openObject;

    private ContainerState containerState;
    private float rotateSpeed;
    private Coroutine rotateRoutine;

    private void Awake()
    {
        containerState = ContainerState.Close;
        rotateSpeed = 10f;
        closeAngleValue = 0f;
    }
    public void Open()
    {

        rotateRoutine = StartCoroutine(OpenCloseRoutine(ContainerState.Open));
    }
    public void Close()
    {

        rotateRoutine = StartCoroutine(OpenCloseRoutine(ContainerState.Close));
    }

    private IEnumerator OpenCloseRoutine(ContainerState newState)
    {
        if (containerState == newState)
            yield break;

        Debug.Log("interact");
        Quaternion targetRot = Quaternion.Euler(openAngleValue, 0f, 0f);
        if (newState == ContainerState.Close)
            targetRot = Quaternion.Euler(closeAngleValue, 0f, 0f);

        while (Quaternion.Angle(openObject.transform.localRotation, targetRot) > 0f)
        {


            float speed = rotateSpeed * Time.deltaTime;

            openObject.transform.localRotation = Quaternion.Slerp(openObject.transform.localRotation, targetRot, rotateSpeed * Time.deltaTime);

            yield return null;
        }


        Debug.Log("stop");
        containerState = newState;

        rotateRoutine = null;
    }

    public override void Detect(out InteractInfo interactInfo)
    {
        interactInfo = new InteractInfo();
        if (containerState == ContainerState.Open)
            interactInfo.interactHint = "아이템 박스 닫기";
        else
            interactInfo.interactHint = "아이템 박스 열기";

    }

    public override void Interact()
    {
        if (rotateRoutine != null)
            return;

        if (containerState == ContainerState.Open)
        {
            Close();
        }
        else
        {

            Open();
        }
    }
}
