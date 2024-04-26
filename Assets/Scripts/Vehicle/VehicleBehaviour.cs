using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class VehicleBehaviour : NetworkBehaviour
{
	[SerializeField] float lookSpeed = 20f;
	TestPlayer player;
	protected VehicleBoarder boarder;
	bool isFirst = true;

	protected Transform cam;

	[Networked, HideInInspector] public float CamYAngle { get; private set; }
	[Networked, HideInInspector] public float CamXAngle { get; private set; }
	[Networked] public NetworkButtons PrevButton { get; private set; }

	protected virtual void Awake()
	{
		boarder = GetComponentInParent<VehicleBoarder>();
		cam = boarder.Cam;
	}

	public void Assign(TestPlayer player)
	{
		if (HasStateAuthority)
		{
			Object.AssignInputAuthority(player.Object.InputAuthority);
			player.Object.RemoveInputAuthority();
		}
		this.player = player;
	}

	public override void FixedUpdateNetwork()
	{
		base.FixedUpdateNetwork();
		if(GetInput(out TestInputData input) == false) { return; }
		NetworkButtons pressed = input.buttons.GetPressed(PrevButton);
		NetworkButtons released = input.buttons.GetReleased(PrevButton);

		PrevButton = input.buttons;
		if(isFirst == true)
		{
			isFirst = false;
			return;
		}

		RotateCam(input);

		if (pressed.IsSet(Buttons.Interact))
		{
			GetOff();
		}
	}

	private void GetOff()
	{
		if (HasStateAuthority)
		{
			player.Object.AssignInputAuthority(Object.InputAuthority);
			Object.RemoveInputAuthority();
		}

		boarder.GetOff(player);
	}

	private void RotateCam(TestInputData input)
	{
		CamYAngle -= input.lookVec.y * Runner.DeltaTime * lookSpeed;
		CamXAngle += input.lookVec.x * Runner.DeltaTime * lookSpeed;
		CamXAngle = Mathf.Repeat(CamXAngle, 360f);
		CamYAngle = Mathf.Clamp(CamYAngle, -40f, 40f);
		
		if(HasInputAuthority)
		{
			cam.rotation = Quaternion.Euler(CamYAngle, CamXAngle, 0f);
		}
	}
}