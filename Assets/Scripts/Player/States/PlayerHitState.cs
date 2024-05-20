using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitState : PlayerState
{
    public PlayerHitState(PlayerController owner) : base(owner)
    {

    }

    public override void Enter()
    {
        owner.movement.StopMove();
        owner.animator.SetTrigger("Hit");
        owner.animator.applyRootMotion = true;
        owner.weaponController.ResetAim();
        owner.weaponController.ChangeHandWeight();

    }

    public override void Exit()
    {
        owner.weaponController.ChangeHandWeight(1f);
        owner.animator.applyRootMotion = false;
    }

    public override void FixedUpdateNetwork()
    {



    }

    public override void Transition()
    {
        int weaponIndex = owner.weaponController.GetMainWeaponAnimLayer();
        AnimatorStateInfo stateInfo = owner.animator.GetCurrentAnimatorStateInfo(weaponIndex);
        if (stateInfo.IsName("Hit") && stateInfo.normalizedTime >= 0.98f)
        {
            ChangeState(PlayerController.PlayerState.StandLocomotion);
            return;
        }
    }
}
