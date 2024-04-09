using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public struct TestInputData : INetworkInput
{
	public const byte MOUSEBUTTON0 = 1;

	public Vector2 moveVec;
	public Vector2 lookVec;
	public NetworkButtons buttons;
}


[DefaultExecutionOrder(-10)]
public class TestInput : NetworkBehaviour, IBeforeUpdate
{
	[SerializeField] float lookSpeed = 30f;

	private TestInputData accumInput;

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
			Vector2 mouseDelta = mouse.delta.ReadValue();

			var lookRotationDelta = new Vector2(mouseDelta.x, mouseDelta.y);
			lookRotationDelta *= lookSpeed / 60f;
			accumInput.lookVec = lookRotationDelta;
			accumInput.buttons.Set(TestInputData.MOUSEBUTTON0, mouse.leftButton.isPressed);
		}

		if (keyboard != null)
		{
			Vector2 moveDirection = Vector2.zero;

			if (keyboard.wKey.isPressed) { moveDirection += Vector2.up; }
			if (keyboard.sKey.isPressed) { moveDirection += Vector2.down; }
			if (keyboard.aKey.isPressed) { moveDirection += Vector2.left; }
			if (keyboard.dKey.isPressed) { moveDirection += Vector2.right; }

			accumInput.moveVec = moveDirection.normalized;
		}
	}
}
