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
        owner.weaponController.ChangeWeaponWeight(0f);
        owner.weaponController.ResetAim();
    }

    public override void Exit()
    {
        owner.animator.SetBool("IsFalling", false);
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
