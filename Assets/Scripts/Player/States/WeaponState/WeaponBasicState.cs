using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBasicState : WeaponStates
{
    protected override void OnEnterState()
    {

        //weaponController.ChangeHandWeight(0);

    }
    protected override void OnExitStateRender()
    {

    }

    protected override void OnFixedUpdate()
    {
        //if (owner.InputListner.pressButton.IsSet(ButtonType.FirstWeapon))
        //{

        //    Machine.TryActivateState((int)WeaponState.Draw);
        //}
        //else if (owner.InputListner.pressButton.IsSet(ButtonType.SecondWeapon))
        //{

        //}
        //else if (owner.InputListner.pressButton.IsSet(ButtonType.SubWeapon))
        //{

        //}
        //else if (owner.InputListner.pressButton.IsSet(ButtonType.MilyWeapon))
        //{

        //}
        //else if (owner.InputListner.pressButton.IsSet(ButtonType.PutWeapon))
        //{
        //}
    }


}
