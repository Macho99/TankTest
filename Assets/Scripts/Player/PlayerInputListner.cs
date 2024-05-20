using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputListner : NetworkBehaviour, IBeforeTick, IBeforeUpdate
{
    public enum PressType { Progress, Press, Release }
    [Networked] public NetworkButtons prevButton { get; private set; }

    [Networked] public NetworkInputData currentInput { get; private set; }
    [Networked] public NetworkButtons pressButton { get; private set; }
    [Networked] public NetworkButtons releaseButton { get; private set; }

    [Networked, Capacity((int)ButtonType.Size)]
    private NetworkDictionary<ButtonType, NetworkBool> limitButton => default;

    NetworkInputData playerInput = new NetworkInputData();
    Vector2Accumulator lookAccum = new Vector2Accumulator(0.02f, true);
    private PlayerControls playerControls;
    public override void Spawned()
    {
        for (int i = 0; i < (int)ButtonType.Size; i++)
        {
            limitButton.Add((ButtonType)i, true);
        }
        if (HasInputAuthority)
        {
            NetworkEvents networkEvents = Runner.GetComponent<NetworkEvents>();
            networkEvents.OnInput.AddListener(OnInput);
        }

    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (HasInputAuthority)
        {
            NetworkEvents networkEvents = Runner.GetComponent<NetworkEvents>();
            networkEvents.OnInput.RemoveListener(OnInput);
        }
    }
    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
        }
        playerControls.Enable();
    }
    private void OnDisable()
    {
        playerControls.Disable();
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        playerInput.inputDirection = playerControls.Player.Move.ReadValue<Vector2>();
        playerInput.buttons.Set(ButtonType.Run, playerControls.Player.Run.IsPressed());
        playerInput.buttons.Set(ButtonType.Jump, playerControls.Player.Jump.IsPressed());
        playerInput.buttons.Set(ButtonType.Crouch, playerControls.Player.Crouch.IsPressed());
        playerInput.buttons.Set(ButtonType.Interact, playerControls.Player.Interact.IsPressed());
        playerInput.buttons.Set(ButtonType.MouseLock, playerControls.Player.TestMouseCursurLock.IsPressed());
        playerInput.buttons.Set(ButtonType.Adherence, playerControls.Player.Adherence.IsPressed());
        playerInput.buttons.Set(ButtonType.ActiveItemContainer, playerControls.Player.ActiveItemContainer.IsPressed());
        playerInput.buttons.Set(ButtonType.PutWeapon, playerControls.Player.PutWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.FirstWeapon, playerControls.Player.FirstWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.SecondWeapon, playerControls.Player.SecondWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.SubWeapon, playerControls.Player.SubWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.MilyWeapon, playerControls.Player.MilyWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.BombWeapon, playerControls.Player.BombWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.Attack, playerControls.Player.Attack.IsPressed());
        playerInput.buttons.Set(ButtonType.Reload, playerControls.Player.Reload.IsPressed());
        playerInput.mouseDelta = lookAccum.ConsumeTickAligned(runner);

        input.Set(playerInput);
        playerInput = default;
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

    public void BeforeUpdate()
    {
        lookAccum.Accumulate(Mouse.current.delta.ReadValue());
    }
}
