using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpStates : PlayerStates
{

    protected override bool CanEnterState()
    {
        if (owner.movement.Kcc.IsGrounded == false)
            return false;



        return true;
    }

    protected override void OnEnterState()
    {
        owner.weaponController.ResetAim();       
        owner.movement.TriggerJump();
        owner.weaponController.ChangeHandWeight();
    }

    protected override void OnEnterStateRender()
    {
        owner.animator.SetBool("IsJump", true);
    }
    protected override void OnExitStateRender()
    {
        owner.animator.SetBool("IsJump", false);
    }
    protected override void OnFixedUpdate()
    {
      
        if (owner.VelocityY <= 3f)
        {
            if (owner.movement.IsGround())
            {
                Machine.TryActivateState((int)PlayerController.PlayerState.Land);
                return;
            }

        }
        else if (owner.VelocityY > 3f)
        {
            Machine.TryActivateState((int)PlayerController.PlayerState.Falling);
            return;
        }
    }
}
