using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : NetworkBehaviour, IBeforeUpdate
{
    [Networked] public NetworkInputData prevInput { get; private set; }

    public void BeforeUpdate()
    {

    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {

        }
    }
    public override void Spawned()
    {

    }


}
