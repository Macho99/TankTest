using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class PlayerStandLocomotionStates : PlayerStates
{
    protected override void OnEnterState()
    {

    }
    protected override void OnFixedUpdate()
    {
        if (GetInput(out NetworkInputData input))
        {
            owner.movement.Rotate(input);
            owner.movement.SetMove(input);
        }

        owner.weaponController.WeaponControls();

        if (owner.InputListner.pressButton.IsSet(ButtonType.Jump))
        {
            //점프상태로 전환
            Machine.TryActivateState((int)PlayerController.PlayerState.Jump);
            return;


        }
        else if (owner.InputListner.pressButton.IsSet(ButtonType.Crouch))
        {
            Machine.TryActivateState((int)PlayerController.PlayerState.CrouchLocomotion);
            return;
        }
    }

}
