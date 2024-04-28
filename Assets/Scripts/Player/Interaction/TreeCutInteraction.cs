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
        isStart = false;
        owner.rigManager.ChangeRigWeight(1f);
        owner.interact.ActiveToolItem(ToolItemType.Hammer, false);
       
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
            interactObject.StartInteract();
            interactObject.onComplete += Complete;
            if (owner.HasInputAuthority)
            {
                operationUI = GameManager.UI.ShowInGameUI<OperationUI>("UI/PlayerUI/OperationUI");
                operationUI.SetTarget(interactObject.transform);
                operationUI.SetOffset(new Vector3(0f, 200f, 0f));
            }

            isStart = true;
        }
        if (isStart)
        {

            if (owner.HasInputAuthority)
            {
                if (interactObject.GetState() == InteractObject.InteractState.Progress)
                {
                    operationUI.SetProgress(interactObject.Progress());

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
        interactObject = owner.interact.InteractObject;

        moveRoutine = owner.StartCoroutine(TreeToPosition());
    }

    public override void InteractStop()
    {
        owner.animator.SetFloat("InteractType", 0f);
        isStart = false;
        interactObject.Stop();
        isStop = true;
        owner.interact.StopInteract();
        operationUI?.CloseUI();
        interactObject.onComplete -= Complete;
        interactObject = null;

    }

    private IEnumerator TreeToPosition()
    {
        Vector3 direction = interactObject.transform.position - owner.transform.position;

        while (direction.magnitude > distance)
        {
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

        moveRoutine = null;
        owner.interact.ActiveToolItem(ToolItemType.Hammer, true);
    }


}
