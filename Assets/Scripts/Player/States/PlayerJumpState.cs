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
        owner.weaponController.ResetAim();
        owner.animator.SetBool("IsJump", true);
        owner.movement.TriggerJump();
        owner.weaponController.ChangeHandWeight();
    }

    public override void Exit()
    {
        owner.weaponController.ChangeHandWeight(1f);
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
                {
                    isJumpingStart = true;
                }
            }
        }


    }

    public override void Transition()
    {
        if (isJumpingStart)
        {
            if (owner.VelocityY <= 3f)
            {
                if (owner.movement.IsGround())
                {
                    ChangeState(PlayerController.PlayerState.Land);
                    return;
                }

            }
            else if (owner.VelocityY > 3f)
            {
                ChangeState(PlayerController.PlayerState.Falling);
                return;
            }






        }
    }


}
