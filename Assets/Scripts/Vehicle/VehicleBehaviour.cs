using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

public class VehicleBehaviour : NetworkBehaviour
{
	const string camPrefabPath = "Vehicle/VehicleFollowerCam";
	const string statUIPrefabPath = "UI/Vehicle/VehicleStatUI";

	[SerializeField] float lookSpeed = 20f;
	PlayerInteract player;
	protected VehicleBoarder boarder;
	bool isFirst = true;

	protected VehicleBody vehicleBody;
	protected VehicleCam followCam;
	protected VehicleStatUI statUI;

	[Networked, OnChangedRender(nameof(PlayerGetOnRender))] public NetworkBool PlayerGetOn { get; private set; } = false;
	[Networked, HideInInspector] public float CamYAngle { get; private set; }
	[Networked, HideInInspector] public float CamXAngle { get; private set; }
	[Networked] public NetworkButtons PrevButton { get; private set; }

	protected virtual void Awake()
	{
		boarder = GetComponentInParent<VehicleBoarder>();
		vehicleBody = boarder.VehicleBody;
	}

	public void Assign(PlayerInteract player)
	{
		PlayerGetOn = true;
		OnAssign(player);
		if(followCam == null)
		{
			followCam = GameManager.Resource.Instantiate<VehicleCam>(camPrefabPath, true);
			followCam.Init(transform, Vector3.up * 2, 8f);
			if (player.HasInputAuthority == false)
			{
				followCam.CamActive(false);
			}
			else
			{
				InstantiateStatUI();
				statUI.Init(vehicleBody.gameObject.name, vehicleBody);
				statUI.AddEvents();
			}
		}

		if (HasStateAuthority)
		{
			Object.AssignInputAuthority(player.Object.InputAuthority);
			player.Object.RemoveInputAuthority();
		}
		this.player = player;
	}

	protected virtual void InstantiateStatUI()
	{
		statUI = GameManager.UI.ShowSceneUI<VehicleStatUI>(statUIPrefabPath);
	}

	protected virtual void OnAssign(PlayerInteract player) { }

	public override void FixedUpdateNetwork()
	{
		base.FixedUpdateNetwork();
		if(GetInput(out NetworkInputData input) == false) { return; }
		NetworkButtons pressed = input.buttons.GetPressed(PrevButton);
		NetworkButtons released = input.buttons.GetReleased(PrevButton);

		PrevButton = input.buttons;
		if(isFirst == true)
		{
			isFirst = false;
			return;
		}

		RotateCam(input);

		if (PlayerGetOn == true && pressed.IsSet(ButtonType.Interact))
		{
			GetOff();
		}
	}

	public void GetOff()
	{
		PlayerGetOn = false;
		OnGetOff();
		if(followCam != null)
		{
			GameManager.Resource.Destroy(followCam.gameObject);
			followCam = null;
		}
		if (statUI != null)
		{
			statUI.RemoveEvents();
			GameManager.Resource.Destroy(statUI.gameObject);
			statUI = null;
		}

		if (HasStateAuthority)
		{
			player.Object?.AssignInputAuthority(Object.InputAuthority);
			Object.RemoveInputAuthority();
		}

		boarder.GetOff(player);
	}

	protected virtual void OnGetOff() { }

	private void RotateCam(NetworkInputData input)
	{
		CamYAngle -= input.mouseDelta.y * Runner.DeltaTime * lookSpeed;
		CamXAngle += input.mouseDelta.x * Runner.DeltaTime * lookSpeed;
		CamXAngle = Mathf.Repeat(CamXAngle, 360f);
		CamYAngle = Mathf.Clamp(CamYAngle, -40f, 40f);
		
		if(HasInputAuthority && followCam != null)
		{
			followCam.transform.rotation = Quaternion.Euler(CamYAngle, CamXAngle, 0f);
		}
	}

	protected virtual void PlayerGetOnRender()
	{
	}
}