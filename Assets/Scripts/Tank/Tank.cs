using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Tank : NetworkBehaviour
{
	[SerializeField] Transform cam;

	TankAttack tankAttack;
	TankMove tankMove;
	TestPlayer player;
	Vector3 position;

	TickTimer getOutTimer;
	[Networked] public NetworkButtons PrevButton { get; private set; }

	private void Awake()
	{
		tankAttack = GetComponent<TankAttack>();
		tankMove = GetComponent<TankMove>();
	}

	public void GetOn(TestPlayer player)
	{
		if (HasStateAuthority)
		{
			Object.AssignInputAuthority(player.Object.InputAuthority);
			player.Object.RemoveInputAuthority();
		}

		this.player = player;
		player.gameObject.SetActive(false);
		player.KCCEnable(false);
		position = transform.InverseTransformPoint(player.transform.position);
		getOutTimer = TickTimer.CreateFromSeconds(Runner, 1f);

		if (HasInputAuthority)
		{
			cam.gameObject.SetActive(true);
		}
	}

	public override void FixedUpdateNetwork()
	{
		if (GetInput(out TestInputData input) == false) return;
		print("input");
		print(input.moveVec);

		NetworkButtons pressed = input.buttons.GetPressed(PrevButton);
		NetworkButtons released = input.buttons.GetReleased(PrevButton);

		PrevButton = input.buttons;

		if (pressed.IsSet(Buttons.Interact))
		{
			GetOff();
		}
	}

	public void GetOff()
	{
		if (getOutTimer.ExpiredOrNotRunning(Runner) == false) return;

		if (HasStateAuthority)
		{
			player.Object.AssignInputAuthority(Object.InputAuthority);
			Object.RemoveInputAuthority();
		}
		player.KCCEnable(true);
		player.Teleport(transform.TransformPoint(position));
		player.gameObject.SetActive(true);
		cam.gameObject.SetActive(false);
		player = null;
	}
}