using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractState : PlayerState
{

    InteractBehavior interactBehavior;

    public PlayerInteractState(PlayerController owner) : base(owner)
    {
    }

    public override void Enter()
    {
        interactBehavior = owner.interact.GetInteractBehavior();
        interactBehavior.endInteract = StopInteract;

        interactBehavior.InteractStart();
    }

    public override void Exit()
    {
        interactBehavior.InteractEnd();
        interactBehavior = null;
    }

    public override void FixedUpdateNetwork()
    {
        interactBehavior.InteractLoop();
    }

    public override void Transition()
    {


    }
    private void StopInteract()
    {
        ChangeState(PlayerController.PlayerState.StandLocomotion);
        return;
    }
}
