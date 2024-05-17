using MoreMountains.Tools;
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

	int breakableLayer;
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
		breakableLayer = LayerMask.NameToLayer("Breakable");
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

	private void InPlaceAttack(AnimationEvent animEvent)
	{
		if (animEvent.animatorClipInfo.weight < 0.5f)
			return;

		Attack(animEvent, transform.position);
	}

	private void FrontAttack(AnimationEvent animEvent)
	{
		if (animEvent.animatorClipInfo.weight < 0.5f)
			return;

		Attack(animEvent, transform.position + transform.forward * animEvent.floatParameter);
	}

	private void PlayCamImpulse(AnimationEvent animEvent)
	{
		if (animEvent.animatorClipInfo.weight < 0.5f)
			return;

		GameManager.Feedback.PlayImpulse(transform.position, 
			StrToVec3(animEvent.stringParameter), 
			animEvent.intParameter, 
			animEvent.floatParameter);
	}

	private void PlayVfx(AnimationEvent animEvent)
	{
		if (animEvent.animatorClipInfo.weight < 0.5f)
			return;

		Vector3 pos = StrToVec3(animEvent.stringParameter);
		GameManager.Resource.Instantiate(animEvent.objectReferenceParameter as GameObject,
			transform.position + transform.rotation * pos, transform.rotation, true);
	}

	protected Vector3 StrToVec3(string str)
	{
		if (str.IsNullOrEmpty())
		{
			Debug.LogError("str 없습니다");
		}
		string[] strVec = str.Replace(" ", "").Split(',');
		if (strVec.Length != 3)
		{
			Debug.LogError($"{str}가 올바른 벡터 형식이 아닙니다");
		}
		return new Vector3(float.Parse(strVec[0]), float.Parse(strVec[1]), float.Parse(strVec[2]));
	}

	private void Attack(AnimationEvent animEvent, Vector3 center)
	{
		int damage = animEvent.intParameter;
		float radius = animEvent.floatParameter;
		Vector3 direction = new();
		if (animEvent.stringParameter.IsNullOrEmpty() == false)
		{
			direction = StrToVec3(animEvent.stringParameter);
			direction.Normalize();
		}
		direction = transform.rotation * direction;
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

			//if (zombieBase.AttackTargetMask.IsLayerInMask(cols[i].gameObject.layer) == true)
			//{
			//	zombieBase.TargetData.SetTarget(cols[i].transform, zombieBase.Runner.Tick);
			//}

			hitList.Add(hittable.HitID);
			float finalForce = force;
			if (cols[i].gameObject.layer == zombieBase.VehicleLayer)
			{
				float mass = cols[i].GetComponentInParent<Rigidbody>().mass;
				finalForce *= 0.25f + (mass / 2000f);
			}
			int finalDamage = damage;
			if (cols[i].gameObject.layer == breakableLayer)
			{
				finalDamage *= 2;
			}
			hittable.ApplyDamage(transform, transform.position, direction * finalForce, finalDamage);
		}
	}
}