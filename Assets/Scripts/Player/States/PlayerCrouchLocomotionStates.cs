using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouchLocomotionStates : PlayerStates
{

    private bool isCrouchToIdleStart;
    private bool isCrouchToIdleEnd;

    protected override bool CanEnterState()
    {
        if (owner.movement.Kcc.IsGrounded == false)
            return false;
        if (owner.movement.CanChanged(PlayerLocomotion.MovementType.Crouch))
            return true;


        return true;
    }

    protected override void OnEnterState()
    {
        owner.movement.ChangeMoveType(PlayerLocomotion.MovementType.Crouch);
    }
    protected override void OnExitState()
    {
        owner.movement.ChangeMoveType(PlayerLocomotion.MovementType.Stand);
        isCrouchToIdleStart = false;
        isCrouchToIdleEnd = false;
    }
    protected override void OnFixedUpdate()
    {
        if (owner.animator.GetCurrentAnimatorStateInfo(0).IsTag("CrouchIdle") || owner.animator.GetCurrentAnimatorStateInfo(0).IsName("CrouchMove") && isCrouchToIdleStart == false)
        {

            owner.movement.Rotate(owner.InputListner.currentInput);
            owner.movement.SetMove(owner.InputListner.currentInput);
            owner.weaponController.WeaponControls();

            if (owner.InputListner.pressButton.IsSet(ButtonType.Crouch) && owner.movement.IsGround())
            {
                if (owner.movement.CanChanged(PlayerLocomotion.MovementType.Stand))
                {
                    isCrouchToIdleStart = true;
                    owner.animator.SetBool("IsCrouch", false);
                    owner.movement.StopMove();

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

        if (isCrouchToIdleEnd)
        {
            owner.movement.ChangeMoveType(PlayerLocomotion.MovementType.Stand);
            Machine.TryActivateState((int)PlayerController.PlayerState.StandLocomotion);
            return;
        }
    }
}
