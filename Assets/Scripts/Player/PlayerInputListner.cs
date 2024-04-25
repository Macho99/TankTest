using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputListner : NetworkBehaviour
{
    public enum PressType { Progress, Press, Release }
    [Networked] public NetworkButtons prevButton { get; private set; }

    [Networked] public NetworkInputData currentInput { get; private set; }
    [Networked] public NetworkButtons pressButton { get; private set; }
    [Networked] public NetworkButtons releaseButton { get; private set; }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            pressButton = input.buttons.GetPressed(prevButton);

            releaseButton = input.buttons.GetReleased(prevButton);

            prevButton = input.buttons;

            currentInput = input;

        }
    }


}