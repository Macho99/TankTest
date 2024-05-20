using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLandStates : PlayerStates
{
    private bool isLandStart;
    private bool isLandEnd;
    protected override void OnEnterState()
    {
        owner.weaponController.ResetAim();
        owner.weaponController.ChangeHandWeight();
        isLandStart = false;
        isLandEnd = false;
    }
    protected override void OnEnterStateRender()
    {
        owner.animator.SetBool("IsLand", true);
        owner.animator.SetFloat("VelocityY", owner.VelocityY);
    }
    protected override void OnExitState()
    {
        owner.weaponController.ChangeHandWeight(1f);
        owner.animator.SetBool("IsLand", false);
        isLandStart = false;
        isLandEnd = false;
        owner.VelocityY = 0f;
        owner.animator.SetFloat("VelocityY", 0f);
    }

    protected override void OnFixedUpdate()
    {
        if (owner.animator.GetCurrentAnimatorStateInfo(0).IsTag("Land"))
        {
            if (!isLandStart)
            {
                owner.movement.StopMove();
                isLandStart = true;
            }

            if (owner.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f && !isLandEnd)
            {
                isLandEnd = true;
            }

        }

        if (isLandEnd)
        {
            Machine.TryActivateState((int)PlayerController.PlayerState.StandLocomotion);
            return;
        }
    }
}
