using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeadState : PlayerState
{
    public PlayerDeadState(PlayerController owner) : base(owner)
    {
    }

    public override void Enter()
    {
        owner.movement.StopMove();
        owner.animator.SetBool("Dead",true);
        owner.animator.applyRootMotion = true;
        owner.weaponController.ResetAim();
        owner.weaponController.ChangeHandWeight();
    }

    public override void Exit()
    {

        owner.animator.SetBool("Dead", false);
        owner.weaponController.ChangeHandWeight(1f);
        owner.animator.applyRootMotion = false;
    }

    public override void FixedUpdateNetwork()
    {
    }

    public override void Transition()
    {
    }

  
}
