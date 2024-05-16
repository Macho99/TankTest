using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;
using Object = UnityEngine.Object;

[RequireComponent(typeof(Animator))]
public class ZombieAnimEvent : MonoBehaviour
{
	const string swingPrefabPath = "FX/VFX/ZombieSwingTrail";
	[SerializeField] protected float swingScale = 1f;
	[SerializeField] protected float force = 60f;

	Collider[] cols = new Collider[10];

	Animator anim;
	Transform lHandTrans;
	Transform rHandTrans;
	ZombieBase zombieBase;
	LayerMask hitMask;
	List<Int64> hitList = new();

	private void Awake()
	{
		anim = GetComponent<Animator>();
		zombieBase = GetComponent<ZombieBase>();
		hitMask = LayerMask.GetMask("Player", "Vehicle", "Breakable");
		lHandTrans = anim.GetBoneTransform(HumanBodyBones.LeftHand);
		rHandTrans = anim.GetBoneTransform(HumanBodyBones.RightHand);
	}

	private void InstantiateSwingVfx(Transform parent, float duration)
	{
		VFXAutoOff vfx = GameManager.Resource.Instantiate<VFXAutoOff>(swingPrefabPath, 
			parent.transform.position, parent.transform.rotation, parent, true);
		vfx.transform.localScale = Vector3.one * swingScale;
		vfx.AutoSetParentNull(duration);
		vfx.SetOffTime(duration + 1f);
	}

	private void TwoHandSwing(AnimationEvent animEvent)
	{
		if(animEvent.animatorClipInfo.weight > 0.5f)
		{
			InstantiateSwingVfx(lHandTrans, animEvent.floatParameter);
			InstantiateSwingVfx(rHandTrans, animEvent.floatParameter);
		}
	}

	private void LeftHandSwing(AnimationEvent animEvent)
	{
		if(animEvent.animatorClipInfo.weight > 0.5f)
		{
			InstantiateSwingVfx(lHandTrans, animEvent.floatParameter);
		}
	}

	private void RightHandSwing(AnimationEvent animEvent)
	{
		if(animEvent.animatorClipInfo.weight > 0.5f)
		{
			InstantiateSwingVfx(rHandTrans, animEvent.floatParameter);
		}
	}

	private void FrontVfx(Object obj)
	{
		VFXAutoOff vfx = obj as VFXAutoOff;
		if(vfx == null)
		{
			Debug.LogError($"{obj}에 VFXAutoOff 컴포넌트를 확인하세요");
		}
	}

	private void FrontAttack(AnimationEvent animEvent)
	{
		if (animEvent.animatorClipInfo.weight < 0.5f)
			return;

		int damage = animEvent.intParameter;
		float radius = animEvent.floatParameter;
		Vector3 direction = new();
		if (animEvent.stringParameter.IsNullOrEmpty() == false)
		{
			string[] strVec = animEvent.stringParameter.Replace(" ", "").Split(',');
			if (strVec.Length != 3)
			{
				Debug.LogError($"{animEvent.stringParameter}가 올바른 벡터 형식이 아닙니다");
			}
			direction = new Vector3(float.Parse(strVec[0]), float.Parse(strVec[1]), float.Parse(strVec[2]));
			direction.Normalize();
		}
		direction = transform.rotation * direction;
		Vector3 center = transform.position + transform.forward * radius;
		int result = Physics.OverlapSphereNonAlloc(center, radius, cols, hitMask);

		if (hitList.Count > 0)
			hitList.Clear();

		for (int i = 0; i < result; i++)
		{
			IHittable hittable = cols[i].GetComponentInParent<IHittable>();
			if (hittable == null) 
				continue;
			if (hitList.Contains(hittable.HitID))
				continue;

			if (zombieBase.AttackTargetMask.IsLayerInMask(cols[i].gameObject.layer) == true)
			{
				zombieBase.TargetData.SetTarget(cols[i].transform, zombieBase.Runner.Tick);
			}

			hitList.Add(hittable.HitID);
			hittable.ApplyDamage(transform, transform.position, direction * force, damage);
		}
	}
}