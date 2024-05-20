using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(BruteZombie))]
public class BruteAnimEvent : ZombieAnimEvent
{
	const string swingDebrisPath = "FX/PoolableDebris/SM_Env_DirtPile_02";
	const string crackDebrisPath = "FX/PoolableDebris/SM_Env_RoadPiece_Damaged_01";
	const string crackVfxPath = "FX/VFX/FX_splash_hit_01_floor";

	[SerializeField] AudioClip[] swingClips;
	[SerializeField] AudioClip crackClip;

	protected override void InstantiateSwingVfx(Transform parent, float duration)
	{
		base.InstantiateSwingVfx(parent, duration);
		GameManager.Sound.PlayOneShot(swingClips[Random.Range(0, swingClips.Length)], AudioGroup.Zombie, transform);
	}

	private void PlayGroundCrack(AnimationEvent animEvent)
	{
		if (animEvent.animatorClipInfo.weight < 0.5f)
			return;

		float scale = animEvent.floatParameter;
		if (scale < 0.01f)
			scale = 1f;
		Vector3 pos = StrToVec3(animEvent.stringParameter);
		Vector3 center = transform.position + transform.rotation * pos;
		GameManager.Resource.Instantiate<PoolableDebrisRoot>(crackDebrisPath, center, transform.rotation, true).
			transform.localScale = Vector3.one * scale;
		GameManager.Resource.Instantiate<FXAutoOff>(crackVfxPath, center, transform.rotation, true).
			transform.localScale = Vector3.one * scale;

		GameManager.Sound.PlayOneShot(crackClip, AudioGroup.Zombie, transform);
	}

	private void PlaySwingDebris(AnimationEvent animEvent)
	{
		if (animEvent.animatorClipInfo.weight < 0.5f)
			return;

		float scale = 1f;
		int dir = animEvent.intParameter;
		Vector3 pos = StrToVec3(animEvent.stringParameter);
		Vector3 center = transform.position + transform.rotation * pos;
		PoolableDebrisRoot poolableDebris = GameManager.Resource.Instantiate<PoolableDebrisRoot>(swingDebrisPath, center, transform.rotation, true);
		poolableDebris.transform.localScale = Vector3.one * scale;
		poolableDebris.Init(1f, 2f);
		poolableDebris.AddRandomVelocityAtPosition(100f * (transform.right * dir + transform.up), center);

		GameManager.Sound.PlayOneShot(crackClip, AudioGroup.Zombie, transform);
	}

	private void PlayClapFeedback(AnimationEvent animEvent)
	{
		if (animEvent.animatorClipInfo.weight < 0.5f)
			return;
		GameManager.Feedback.PlayClap(transform.position, 20f);

		GameManager.Sound.PlayOneShot(crackClip, AudioGroup.Zombie, transform);
	}
}