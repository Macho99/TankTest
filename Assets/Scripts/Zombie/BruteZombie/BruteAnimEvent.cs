using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(BruteZombie))]
public class BruteAnimEvent : ZombieAnimEvent
{
	const string crackDebrisPath = "FX/VFX/SM_Env_RoadPiece_Damaged_01";
	const string crackVfxPath = "FX/VFX/FX_splash_hit_01_floor";

	private void PlayGroundCrack(AnimationEvent animEvent)
	{
		if (animEvent.animatorClipInfo.weight < 0.5f)
			return;

		float scale = animEvent.floatParameter;
		if (scale < 0.01f)
			scale = 1f;
		Vector3 pos = StrToVec3(animEvent.stringParameter);
		Vector3 center = transform.position + transform.rotation * pos;
		GameManager.Resource.Instantiate<DebrisRoot>(crackDebrisPath, center, transform.rotation, false).
			transform.localScale = Vector3.one * scale;
		GameManager.Resource.Instantiate<VFXAutoOff>(crackVfxPath, center, transform.rotation, true).
			transform.localScale = Vector3.one * scale;
	}
}