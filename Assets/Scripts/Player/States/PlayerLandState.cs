using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLandState : PlayerState
{
    private bool isLandStart;
    private bool isLandEnd;
    public PlayerLandState(PlayerController owner) : base(owner)
    {
    }

    public override void Enter()
    {
        owner.weaponController.ResetAim();
        owner.weaponController.ChangeWeaponWeight(0f); 
        owner.animator.SetBool("IsLand",true);
        owner.animator.SetFloat("VelocityY", owner.VelocityY);
        Debug.Log(owner.VelocityY);
        isLandStart = false;
        isLandEnd = false;
    }


    public override void Exit()
    {
        owner.weaponController.ChangeWeaponWeight(1f);
        owner.animator.SetBool("IsLand", false);
        isLandStart = false;
        isLandEnd = false;
        owner.VelocityY = 0f;
        owner.animator.SetFloat("VelocityY", 0f);
    }

    public override void FixedUpdateNetwork()
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
    }

    public override void Transition()
    {
        if (isLandEnd)
        {
            
            ChangeState(PlayerController.PlayerState.StandLocomotion);
            return;
        }
    }


}
