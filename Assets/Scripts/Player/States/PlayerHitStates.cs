using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitStates : PlayerStates
{
    [Networked] private NetworkBool isHitStart { get; set; }

    protected override void OnEnterState()
    {
        owner.movement.Kcc.SetActive(false);
        owner.weaponController.ResetAim();
        owner.weaponController.ChangeHandWeight();
        owner.movement.StopMove();

    }
    protected override void OnEnterStateRender()
    {
      
        owner.animator.applyRootMotion = true;
        owner.animator.SetBool("Hit", true);
    }

    protected override void OnExitState()
    {
        owner.movement.Kcc.SetActive(true);
        owner.movement.Kcc.SetPosition(owner.transform.position);
        owner.weaponController.ChangeHandWeight(1f);

        isHitStart = false;
    }
    protected override void OnExitStateRender()
    {
        owner.animator.SetBool("Hit", false);
        owner.animator.applyRootMotion = false;

    }
    protected override void OnFixedUpdate()
    {
        int weaponIndex = owner.weaponController.GetMainWeaponAnimLayer();
        AnimatorStateInfo stateInfo = owner.animator.GetCurrentAnimatorStateInfo(weaponIndex);
        print(weaponIndex);
        if (stateInfo.IsName("Hit"))
        {
            isHitStart = true;
            Debug.Log("Hit");
        }

        Debug.Log(stateInfo.normalizedTime.ToString("F1"));

        if (isHitStart)
        {
            if (stateInfo.normalizedTime >= 0.95f)
            {
                Machine.TryActivateState((int)PlayerController.PlayerState.StandLocomotion);
                return;

            }
        }
    }
}
