using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStandLocomotionState : PlayerState
{

    public PlayerStandLocomotionState(PlayerController owner) : base(owner)
    {
    }

    public override void Enter()
    {
        owner.movement.ChangeMoveType(PlayerLocomotion.MovementType.Stand);
    }

    public override void Exit()
    {

    }

    public override void FixedUpdateNetwork()
    {
        if (owner.GetInput(out NetworkInputData input))
        {
            owner.movement.Rotate(input);
            owner.movement.SetMove(input);
        }

    }

    public override void SetUp()
    {

    }

    public override void Transition()
    {
        if (owner.GetInput(out NetworkInputData input))
        {
            if (input.buttons.IsSet(NetworkInputData.ButtonType.Jump) && owner.movement.IsGround())
            {
                //점프상태로 전환
                ChangeState(PlayerController.PlayerState.Jump);
                return;
            }
            else if (input.buttons.IsSet(NetworkInputData.ButtonType.Crouch) && owner.movement.IsGround())
            {
                if (owner.movement.CanChanged(PlayerLocomotion.MovementType.Crouch))
                {
                    ChangeState(PlayerController.PlayerState.CrouchLocomotion);
                    return;
                }
            }
        }

    }

}
