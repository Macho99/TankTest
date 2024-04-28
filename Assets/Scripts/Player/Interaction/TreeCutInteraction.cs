using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeCutInteraction : InteractBehavior
{
    private OperationUI operationUI;
    private float distance;
    private Coroutine moveRoutine;
    private bool isStop;
    public TreeCutInteraction(PlayerController owner) : base(owner, InteractType.TreeCut)
    {
        distance = 1f;

    }


    public override void InteractEnd()
    {
        if (isStop == false)
            InteractStop();

    }

    public override void InteractLoop()
    {
        if (isStart)
        {
            if (interactObject.GetState() == InteractObject.InteractState.Progress)
            {
                operationUI.SetProgress(interactObject.Progress());
            }



            if (owner.InputListner.pressButton.IsSet(NetworkInputData.ButtonType.Interact))
            {
                InteractStop();

                return;
            }

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
        interactObject = owner.interact.InteractObject;
        moveRoutine = owner.StartCoroutine(TreeToPosition());
    }

    public override void InteractStop()
    {
        owner.rigManager.ChangeRigWeight(1f);
        owner.animator.SetFloat("InteractType", 0f);

        interactObject.Stop();

        if (owner.HasInputAuthority)
            operationUI?.CloseUI();


        isStart = false;
        isStop = true;
        owner.interact.InteractObject = null;
        interactObject = null;
        endInteract?.Invoke();
    }

    private IEnumerator TreeToPosition()
    {
        Vector3 direction = interactObject.transform.position - owner.transform.position;

        while (direction.magnitude > distance)
        {
            owner.movement.Rotate(direction.normalized);


            owner.movement.SetMove(direction.normalized);
            direction = interactObject.transform.position - owner.transform.position;
            yield return null;
        }

        if (interactObject.GetState() == InteractObject.InteractState.Progress)
        {
            InteractStop();
            yield break;
        }

        owner.animator.SetFloat("InteractType", (int)interactType);

        owner.movement.SetMove(Vector3.zero);
        interactObject.StartInteract();
        if (owner.HasInputAuthority)
        {
            operationUI = GameManager.UI.ShowInGameUI<OperationUI>("UI/PlayerUI/OperationUI");
            operationUI.SetTarget(interactObject.transform);
            operationUI.SetOffset(new Vector3(0f, 200f, 0f));
        }

        isStart = true;
        moveRoutine = null;

    }
}
