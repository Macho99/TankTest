using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputListner : NetworkBehaviour
{
    public enum PressType { Progress, Press, Release }
    [Networked] public NetworkButtons prevButton { get; private set; }

    [Networked] public NetworkButtons pressButton { get; private set; }
    [Networked] public NetworkButtons releaseButton { get; private set; }

    [Networked, Capacity((int)ButtonType.Size)]
    private NetworkDictionary<ButtonType, NetworkBool> limitButton => default;


 
    public override void FixedUpdateNetwork()
    {
		//if (IsInputListner == false)
		//    return;

		if (GetInput(out NetworkInputData input))
		{

			//input = LimitButton(input);

			pressButton = input.buttons.GetPressed(prevButton);

			releaseButton = input.buttons.GetReleased(prevButton);

			prevButton = input.buttons;

		}
        else
        {
            pressButton = default;
            releaseButton = default;
        }
	}


}
