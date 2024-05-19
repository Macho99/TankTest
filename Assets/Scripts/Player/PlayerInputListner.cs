using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputListner : NetworkBehaviour,IBeforeTick
{
    public enum PressType { Progress, Press, Release }
    [Networked] public NetworkButtons prevButton { get; private set; }

    [Networked] public NetworkInputData currentInput { get; private set; }
    [Networked] public NetworkButtons pressButton { get; private set; }
    [Networked] public NetworkButtons releaseButton { get; private set; }

    [Networked, Capacity((int)ButtonType.Size)]
    private NetworkDictionary<ButtonType, NetworkBool> limitButton => default;
    public override void Spawned()
    {
        for (int i = 0; i < (int)ButtonType.Size; i++)
        {
            limitButton.Add((ButtonType)i, true);
        }
    }
    public override void FixedUpdateNetwork()
    {
        //if (IsInputListner == false)
        //    return;

      
    }

    private NetworkInputData LimitButton(NetworkInputData input)
    {
        input.inputDirection = limitButton.Get(ButtonType.PlayerMove) == false ? Vector2.zero : input.inputDirection;
        if (limitButton.Get(ButtonType.MouseDelta) == false)
        {
            if (HasInputAuthority)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            input.mouseDelta = Vector2.zero;
            input.buttons.Set(ButtonType.Adherence, false);
        }
        else
        {
            if (HasInputAuthority)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        for (int i = (int)ButtonType.Run; i < (int)ButtonType.Size; i++)
        {
            input.buttons.Set(button: (ButtonType)i, limitButton.Get((ButtonType)i) == false ? false : input.buttons.IsSet((ButtonType)i));
        }


        return input;
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ActiveButton(ButtonType button, bool isActive)
    {
        limitButton.Set(button, isActive);
    }

    public void BeforeTick()
    {
        if (GetInput(out NetworkInputData input))
        {

            input = LimitButton(input);

            pressButton = input.buttons.GetPressed(prevButton);

            releaseButton = input.buttons.GetReleased(prevButton);

            prevButton = input.buttons;

            currentInput = input;

        }
    }
}
