using Fusion.Addons.FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class PlayerStates : StateBehaviour<PlayerStates>
{
    [SerializeField]protected PlayerController owner;
    //public PlayerStates(PlayerController controller)
    //{
    //    this.owner = controller;
    //}

}
