using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public enum Buttons { Fire, Jump }

public struct TestInputData : INetworkInput
{

	public Vector2 moveVec;
	public Vector2 lookVec;
	public NetworkButtons buttons;
}


[DefaultExecutionOrder(-10)]
public class TestInput : NetworkBehaviour, IBeforeUpdate
{
	private TestInputData accumInput;
	private Vector2 accumLookVec = new();

	public override void Spawned()
	{
		if (HasInputAuthority == false)
			return;

		// Register to Fusion input poll callback.
		var networkEvents = Runner.GetComponent<NetworkEvents>();
		networkEvents.OnInput.AddListener(OnInput);

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	public void OnInput(NetworkRunner runner, NetworkInput input)
	{
		input.Set(accumInput);
	}

	private void Update()
	{
		var mouse = Mouse.current;
		if (mouse != null)
		{
			accumLookVec += mouse.delta.ReadValue();
		}
	}

	public void BeforeUpdate()
	{
		if (Object.HasInputAuthority == false)
			return;

		// Enter key is used for locking/unlocking cursor in game view.
		var keyboard = Keyboard.current;
		if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
		{
			if (Cursor.lockState == CursorLockMode.Locked)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			else
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}

		// Accumulate input only if the cursor is locked.
		if (Cursor.lockState != CursorLockMode.Locked)
			return;

		var mouse = Mouse.current;
		if (mouse != null)
		{
			accumInput.lookVec = accumLookVec;
			accumLookVec = new();
			accumInput.buttons.Set(Buttons.Fire, mouse.leftButton.isPressed);
		}

		if (keyboard != null)
		{
			Vector2 moveDirection = Vector2.zero;

			if (keyboard.wKey.isPressed) { moveDirection += Vector2.up; }
			if (keyboard.sKey.isPressed) { moveDirection += Vector2.down; }
			if (keyboard.aKey.isPressed) { moveDirection += Vector2.left; }
			if (keyboard.dKey.isPressed) { moveDirection += Vector2.right; }

			accumInput.buttons.Set(Buttons.Jump, keyboard.spaceKey.isPressed);

			accumInput.moveVec = moveDirection.normalized;
		}
	}
}