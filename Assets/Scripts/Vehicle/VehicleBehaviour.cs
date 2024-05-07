using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class VehicleBehaviour : NetworkBehaviour
{
	const string camPrefabPath = "Vehicle/VehicleFollowerCam";

	[SerializeField] float lookSpeed = 20f;
	TestPlayer player;
	protected VehicleBoarder boarder;
	bool isFirst = true;

	protected VehicleCam followCam;

	[Networked, HideInInspector] public float CamYAngle { get; private set; }
	[Networked, HideInInspector] public float CamXAngle { get; private set; }
	[Networked] public NetworkButtons PrevButton { get; private set; }

	protected virtual void Awake()
	{
		boarder = GetComponentInParent<VehicleBoarder>();
		//cam = boarder.Cam;
	}

	public void Assign(TestPlayer player)
	{
		OnAssign(player);
		if(Runner.IsForward)
		{
			followCam = GameManager.Resource.Instantiate<VehicleCam>(camPrefabPath, true);
			followCam.Init(transform, Vector3.up * 2, 4f);
			if (player.HasInputAuthority == false)
			{
				followCam.CamActive(false);
			}
		}

		if (HasStateAuthority)
		{
			Object.AssignInputAuthority(player.Object.InputAuthority);
			player.Object.RemoveInputAuthority();
		}
		this.player = player;
	}

	protected virtual void OnAssign(TestPlayer player) { }

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
		OnGetOff();
		if (Runner.IsForward)
		{
			GameManager.Resource.Destroy(followCam.gameObject);
		}

		if (HasStateAuthority)
		{
			player.Object.AssignInputAuthority(Object.InputAuthority);
			Object.RemoveInputAuthority();
		}

		boarder.GetOff(player);
	}

	protected virtual void OnGetOff() { }

	private void RotateCam(TestInputData input)
	{
		CamYAngle -= input.lookVec.y * Runner.DeltaTime * lookSpeed;
		CamXAngle += input.lookVec.x * Runner.DeltaTime * lookSpeed;
		CamXAngle = Mathf.Repeat(CamXAngle, 360f);
		CamYAngle = Mathf.Clamp(CamYAngle, -40f, 40f);
		
		if(HasInputAuthority)
		{
			followCam.transform.rotation = Quaternion.Euler(CamYAngle, CamXAngle, 0f);
		}
	}
}