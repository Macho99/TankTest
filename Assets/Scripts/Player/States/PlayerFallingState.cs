using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallingState : PlayerState
{
    public PlayerFallingState(PlayerController owner) : base(owner)
    {
    }

    public override void Enter()
    {
        owner.animator.SetBool("IsFalling", true);
        owner.UpperLayerWeight = 0f;
        owner.rigManager.LeftHandweight = 0f;
    }

    public override void Exit()
    {
        owner.animator.SetBool("IsFalling", false);
        owner.UpperLayerWeight = 1f;
        owner.rigManager.LeftHandweight = 1f;
    }

    public override void FixedUpdateNetwork()
    {

    }

    public override void Transition()
    {
        if (owner.movement.IsGround())
        {
            ChangeState(PlayerController.PlayerState.Land);
            return;
        }
    }


}
