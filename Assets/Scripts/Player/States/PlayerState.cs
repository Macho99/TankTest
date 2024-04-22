using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerState : NetworkBaseState
{
    protected PlayerController owner;
   
    public PlayerState(PlayerController owner)
    {
        this.owner = owner;
    }

    public override void SetUp()
    {
        
    }
}
