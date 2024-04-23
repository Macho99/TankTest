using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbPassState : PlayerState
{
    public PlayerClimbPassState(PlayerController owner) : base(owner)
    {
    }

    public void ChangeState(RaycastHit hitInfo)
    {

        Vector3 newposition = hitInfo.point + hitInfo.normal;
        Vector3 direction = (newposition - owner.transform.position).normalized;


        ChangeState(PlayerController.PlayerState.ClimbPass);
        return;
    }
    public override void Enter()
    {

    }

    public override void Exit()
    {
    }

    public override void FixedUpdateNetwork()
    {
    }

    public override void Transition()
    {
    }


}
