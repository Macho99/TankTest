using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerState
{
    private bool isJumpingStart;
    public PlayerJumpState(PlayerController owner) : base(owner)
    {

    }

    public override void Enter()
    {
        owner.animator.SetBool("IsJump", true);
        owner.movement.TriggerJump();
    }

    public override void Exit()
    {
        owner.animator.SetBool("IsJump", false);
        isJumpingStart = false;
    }

    public override void FixedUpdateNetwork()
    {
        if (owner.animator.GetCurrentAnimatorStateInfo(0).IsTag("Jump"))
        {
            if (!isJumpingStart)
            {
                if (!owner.RaycastGroundCheck())
                    isJumpingStart = true;
            }
        }


    }

    public override void Transition()
    {
        if (isJumpingStart)
        {

            if (owner.movement.IsGround())
            {
                if (owner.FallingTime <= 3f)
                {
                    owner.FallingTime = 0f;
                    ChangeState(PlayerController.PlayerState.StandLocomotion);
                }

                return;
            }







        }
    }


}
