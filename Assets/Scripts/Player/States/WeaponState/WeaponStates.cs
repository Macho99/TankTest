using Fusion.Addons.FSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponStates : StateBehaviour<WeaponStates>
{
    [SerializeField] protected WeaponController owner;
}
