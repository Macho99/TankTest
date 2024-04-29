using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmingInteraction : InteractBehavior
{
    private OperationUI operationUI;
    private float distance;
    private Coroutine moveRoutine;
    private bool isStop;
    public FarmingInteraction(PlayerController owner, InteractType interactType) : base(owner, interactType)
    {
        distance = owner.interact.ObjectDistance;
    }


    public override void InteractEnd()
    {
        isStart = false;
        owner.rigManager.ChangeRigWeight(1f);
        owner.interact.ActiveToolItem(interactType, false);

        if (moveRoutine != null)
        {
            owner.StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
    }

    public override void InteractLoop()
    {
        if (owner.animator.GetCurrentAnimatorStateInfo(0).IsName("InteractStart") && isStart == false && isStop == false)
        {
            owner.interact.InteractObject.StartInteract();
            owner.interact.InteractObject.onComplete += Complete;
            if (owner.HasInputAuthority)
            {
                operationUI = GameManager.UI.ShowInGameUI<OperationUI>("UI/PlayerUI/OperationUI");
                operationUI.SetTarget(owner.interact.InteractObject.transform);
                operationUI.SetOffset(new Vector3(0f, 200f, 0f));
            }

            isStart = true;
        }
        if (isStart)
        {

            if (owner.HasInputAuthority)
            {
                if (owner.interact.InteractObject.GetState() == InteractObject.InteractState.Progress)
                {
                    operationUI.SetProgress(owner.interact.InteractObject.Progress());

                }
            }

            if (owner.InputListner.pressButton.IsSet(NetworkInputData.ButtonType.Interact))
            {
                InteractStop();

                return;
            }

        }

        if (isStop && owner.animator.GetCurrentAnimatorStateInfo(0).IsName("InteractEnd") && owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
        {
            endInteract?.Invoke();
            isStop = false;
        }

    }

    public override void InteractStart()
    {
        owner.rigManager.ChangeRigWeight(0f);
        if (moveRoutine != null)
        {
            owner.StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        moveRoutine = owner.StartCoroutine(MoveToFarmingObject());
    }

    public override void InteractStop()
    {
        owner.animator.SetFloat("InteractType", 0f);
        owner.interact.InteractObject.onComplete -= Complete;
        isStart = false;
        owner.interact.InteractObject.interactState = InteractObject.InteractState.None;
        isStop = true;
        owner.interact.StopInteract();
        operationUI?.CloseUI();

    }

    private IEnumerator MoveToFarmingObject()
    {
        Vector3 direction = owner.interact.targetPoint - owner.transform.position;
        direction = new Vector3(direction.x, 0f, direction.z);

        Debug.Log(direction.magnitude);
        while (direction.magnitude > distance)
        {
            owner.movement.SetMove(direction.normalized);
            direction = owner.interact.targetPoint - owner.transform.position;
            direction = new Vector3(direction.x, 0f, direction.z);
            yield return null;
        }

        if (owner.interact.InteractObject.GetState() == InteractObject.InteractState.Progress)
        {
            InteractStop();
            yield break;
        }

        owner.animator.SetFloat("InteractType", (int)interactType);

        owner.movement.SetMove(Vector3.zero);

        moveRoutine = null;
        owner.interact.ActiveToolItem(interactType, true);
    }


}
