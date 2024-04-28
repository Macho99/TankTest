using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerStandLocomotionState : PlayerState
{

    public PlayerStandLocomotionState(PlayerController owner) : base(owner)
    {
    }

    public override void Enter()
    {

    }

    public override void Exit()
    {

    }

    public override void FixedUpdateNetwork()
    {

        owner.movement.Rotate(owner.InputListner.currentInput);
        owner.movement.SetMove(owner.InputListner.currentInput);


    }

    public override void SetUp()
    {

    }
    public override void Transition()
    {

        if (owner.InputListner.pressButton.IsSet(NetworkInputData.ButtonType.Jump) && owner.movement.IsGround())
        {

            //점프상태로 전환
            ChangeState(PlayerController.PlayerState.Jump);
            return;


        }
        else if (owner.InputListner.pressButton.IsSet(NetworkInputData.ButtonType.Crouch) && owner.movement.IsGround())
        {
            if (owner.movement.CanChanged(PlayerLocomotion.MovementType.Crouch))
            {
                owner.movement.ChangeMoveType(PlayerLocomotion.MovementType.Crouch);
                ChangeState(PlayerController.PlayerState.CrouchLocomotion);
                return;
            }
        }

        if (owner.GetInput(out NetworkInputData input))
        {
            if (owner.InputListner.pressButton.IsSet(NetworkInputData.ButtonType.Interact))
            {
                if (owner.interact.IsDetect)
                {

                    if (owner.interact.TryInteract())
                    {
                        ChangeState(PlayerController.PlayerState.Interact);
                        return;

                    }
                }

            }
        }



    }

}
