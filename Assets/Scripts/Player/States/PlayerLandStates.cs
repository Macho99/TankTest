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
        isLandStart = false;
        isLandEnd = false;
        owner.VelocityY = 0f;
       
    }
    protected override void OnExitStateRender()
    {
        owner.animator.SetBool("IsLand", false);
        owner.animator.SetFloat("VelocityY", 0f);
    }

    protected override void OnFixedUpdate()
    {

        AnimatorStateInfo stateInfo = owner.animator.GetCurrentAnimatorStateInfo(owner.weaponController.GetMainWeaponAnimLayer());
        if (stateInfo.IsTag("Land"))
        {
            if (!isLandStart)
            {
                owner.movement.StopMove();
                isLandStart = true;
            }

            if (stateInfo.normalizedTime >= 0.95f && !isLandEnd)
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
