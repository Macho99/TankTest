using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouchLocomotionState : PlayerState
{
    private bool isCrouchToIdleStart;
    private bool isCrouchToIdleEnd;
    public PlayerCrouchLocomotionState(PlayerController owner) : base(owner)
    {
    }

    public override void Enter()
    {
    
    }

    public override void Exit()
    {
        isCrouchToIdleStart = false;
        isCrouchToIdleEnd = false;
    }

    public override void FixedUpdateNetwork()
    {
        if (owner.animator.GetCurrentAnimatorStateInfo(0).IsTag("CrouchIdle") || owner.animator.GetCurrentAnimatorStateInfo(0).IsTag("CrouchMove") && isCrouchToIdleStart == false)
        {
            if (owner.GetInput(out NetworkInputData input))
            {
                owner.movement.Rotate(input);
                owner.movement.SetMove(input);

                if (input.buttons.IsSet(NetworkInputData.ButtonType.Crouch) && owner.movement.IsGround())
                {
                    if (owner.movement.CanChanged(PlayerLocomotion.MovementType.Stand))
                    {
                        isCrouchToIdleStart = true;
                        owner.animator.SetBool("IsCrouch", false);
                        owner.movement.StopMove();

                    }
                }
            }
        }
        else if (isCrouchToIdleStart)
        {
            if (owner.animator.GetCurrentAnimatorStateInfo(0).IsName("CrouchToIdle") && owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f && isCrouchToIdleEnd == false)
            {
                isCrouchToIdleEnd = true;
                Debug.Log("asdada");
            }
        }
    }
    public override void Transition()
    {
        if (isCrouchToIdleEnd)
        {
            owner.movement.ChangeMoveType(PlayerLocomotion.MovementType.Stand);
            ChangeState(PlayerController.PlayerState.StandLocomotion);
            return;
        }

    }

}
