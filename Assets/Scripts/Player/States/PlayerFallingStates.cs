using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallingStates : PlayerStates
{

    protected override void OnEnterState()
    {
      
        owner.weaponController.ChangeHandWeight(0f);
        owner.weaponController.ResetAim();
    }

    protected override void OnEnterStateRender()
    {
        owner.animator.SetBool("IsFalling", true);
    }
    protected override void OnExitStateRender()
    {
        owner.animator.SetBool("IsFalling", false);
    }
    protected override void OnFixedUpdate()
    {
        if (owner.movement.IsGround())
        {
            Machine.TryActivateState((int)PlayerController.PlayerState.Land);
            return;
        }

    }
}
