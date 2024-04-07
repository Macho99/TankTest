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
			aimImg.color = Color.red;
			Fire();
		}
		if (released.IsSet(TestInputData.MOUSEBUTTON0))
		{
			print("¶À");
			aimImg.color = Color.white;
		}
	}

	private void Fire()
	{
		if(Runner.LagCompensation.Raycast(transform.position, transform.forward, 100f, 
			Object.InputAuthority, out var hit, LayerMask.GetMask("Monster"), HitOptions.IgnoreInputAuthority))
		{
			if (hit.Hitbox == null)
			{
				return;
			}

			if (hit.Hitbox is ZombieHitBox zombieHitBox)
			{
				zombieHitBox.ApplyDamage(-transform.forward, 1f, 1);
			}
		}
	}
}
