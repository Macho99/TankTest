using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeadStates : PlayerStates
{
    protected override void OnEnterState()
    {
        owner.movement.StopMove();
        owner.movement.Kcc.SetActive(false);
        owner.weaponController.ResetAim();
        owner.weaponController.ChangeHandWeight();
    }
    protected override void OnEnterStateRender()
    {
        owner.animator.SetBool("Dead", true);
        owner.animator.applyRootMotion = true;
    }
    protected override void OnExitStateRender()
    {
        owner.animator.SetBool("Dead", false);
        owner.animator.applyRootMotion = false;
    }
    protected override void OnExitState()
    {
        owner.movement.Kcc.SetPosition(owner.transform.position);
        owner.movement.Kcc.SetActive(true);

        owner.weaponController.ChangeHandWeight(1f);
    }
}
