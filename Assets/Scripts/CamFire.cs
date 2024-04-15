using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class CamFire : NetworkBehaviour
{
	[SerializeField] Image aimImg;

	[Networked] NetworkButtons PrevButton { get; set; }

	public override void Spawned()
	{
		if (Runner.IsServer)
		{
			Object.AssignInputAuthority(Runner.LocalPlayer);
		}
	}

	public override void FixedUpdateNetwork()
	{
		if (GetInput(out TestInputData data) == false) return;

		NetworkButtons pressed = data.buttons.GetPressed(PrevButton);
		NetworkButtons released = data.buttons.GetReleased(PrevButton);

		PrevButton = data.buttons;

		if (pressed.IsSet(Buttons.Fire))
		{
			print("누름");
			aimImg.color = Color.red;
			Fire();
		}
		if (released.IsSet(Buttons.Fire))
		{
			print("뗌");
			aimImg.color = Color.white;
		}
	}

	private void Fire()
	{
		if(Runner.LagCompensation.Raycast(transform.position, transform.forward, 100f, 
			Object.InputAuthority, out var hit))
		{
			if (hit.Hitbox == null)
			{
				return;
			}

			if (hit.Hitbox is ZombieHitBox zombieHitBox)
			{
				zombieHitBox.ApplyDamage(-transform.forward, 1);
			}
		}
		else
		{
			print("안맞음");
		}
	}
}
