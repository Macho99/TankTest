using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


public class CamFire : NetworkBehaviour
{
	[Networked] NetworkButtons PrevButton { get; set; }

	public override void Spawned()
	{
		Object.AssignInputAuthority(Runner.LocalPlayer);
	}

	public override void FixedUpdateNetwork()
	{
		if (GetInput(out TestInputData data) == false) return;

		NetworkButtons pressed = data.buttons.GetPressed(PrevButton);
		NetworkButtons released = data.buttons.GetReleased(PrevButton);

		PrevButton = data.buttons;

		if (pressed.IsSet(TestInputData.MOUSEBUTTON0))
		{
			print("´©¸§");
		}
		if (released.IsSet(TestInputData.MOUSEBUTTON0))
		{
			print("¶À");
		}
	}
}
