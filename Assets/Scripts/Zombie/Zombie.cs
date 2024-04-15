using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;
using Random = UnityEngine.Random;

public struct BoneTransform
{
	public Vector3 localPosition;
	public Quaternion localRotation;
}

public class Zombie : NetworkBehaviour
{
	public struct BodyPart
	{
		public ZombieHitBox zombieHitBox;
		public Rigidbody rb;
		public Collider col;
	}

	public enum State { Idle, Wander, Trace, AnimWait, Wait, CrawlIdle, RagdollEnter, RagdollExit }
	[SerializeField] float minIdleTime = 1f;
	[SerializeField] float maxIdleTime = 10f;
	[SerializeField] Transform skins;
	[SerializeField] float fallAsleepThreshold = 0.2f;
	[SerializeField] TextMeshProUGUI curStateText;

	NavMeshAgent agent;
	NetworkStateMachine stateMachine;
	Animator anim;
	Rigidbody[] rbs;
	Collider[] cols;

	BodyPart[] bodyParts = new BodyPart[(int) ZombieBody.Size];

	public Transform[] Bones { get; private set; }
	public BoneTransform[] RagdollBoneTransforms { get; private set; }
	public static BoneTransform[] FaceUpBoneTransforms { get; private set; }
	public static BoneTransform[] FaceDownBoneTranforms { get; private set; }
	public BodyPart[] BodyParts { get { return bodyParts; } }
	public Transform Hips { get; private set; }
	public Animator Anim { get { return anim; } }
	public float FallAsleepThreshold { get { return fallAsleepThreshold; } }
	public NavMeshAgent Agent { get { return agent; } }
	public LayerMask FallAsleepMask { get; set; }
	//	#region Variable For Specific State
	// Idle State
	public float MinIdleTime { get { return minIdleTime; } }
	public float MaxIdleTime { get { return maxIdleTime; } }

	// AnimWait State
	public AnimWaitStruct? AnimWaitStruct { get; set; }
	//	#endregion

	public NetworkObject Target { get; private set; }
	public float TraceSpeed { get; private set; }

	[Networked] public int SkinIdx { get; private set; }
	[Networked] public Vector3 Position { get; set; }
	[Networked] public Quaternion Rotation { get; set; }
	[Networked, OnChangedRender(nameof(RagdollChanged))] public NetworkBool IsRagdoll { get; set; }
	[Networked] public ZombieBody RagdollBody { get; private set; }
	[Networked] public Vector3 RagdollVelocity { get; private set; }

	public Vector3 LastFootPos { get; set; }

	private void Awake()
	{
		FallAsleepMask = LayerMask.GetMask("FallAsleepObject");
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();

		Hips = anim.GetBoneTransform(HumanBodyBones.Hips);
		Bones = Hips.GetComponentsInChildren<Transform>();
		RagdollBoneTransforms = new BoneTransform[Bones.Length];
		if(FaceDownBoneTranforms == null)
		{
			InitFaceBoneTransforms();
		}

		stateMachine = GetComponent<NetworkStateMachine>();

		stateMachine.AddState(State.Idle, new ZombieIdle(this));
		stateMachine.AddState(State.Trace, new ZombieTrace(this));
		stateMachine.AddState(State.Wander, new ZombieWander(this));
		stateMachine.AddState(State.AnimWait, new ZombieAnimWait(this));
		stateMachine.AddState(State.CrawlIdle, new ZombieCrawlIdle(this));
		stateMachine.AddState(State.Wait, new ZombieWait(this));
		stateMachine.AddState(State.RagdollEnter, new ZombieRagdollEnter(this));
		stateMachine.AddState(State.RagdollExit, new ZombieRagdollExit(this));

		stateMachine.InitState(State.Idle);

		SetBodyParts();
		SetRbKinematic(false);
	}

	public void CopyBoneTransforms(BoneTransform[] boneTransforms)
	{
		for (int i = 0; i < Bones.Length; i++)
		{
			boneTransforms[i] = new BoneTransform
			{
				localPosition = Bones[i].localPosition,
				localRotation = Bones[i].localRotation
			};
		}
	}

	private void InitFaceBoneTransforms()
	{
		FaceUpBoneTransforms = new BoneTransform[Bones.Length];
		FaceDownBoneTranforms = new BoneTransform[Bones.Length];

		SampleFaceBoneTransforms("FaceUp", FaceUpBoneTransforms);
		SampleFaceBoneTransforms("FaceDown", FaceDownBoneTranforms);
	}

	private void SampleFaceBoneTransforms(string clipName, BoneTransform[] boneTransforms)
	{
		Vector3 prevPosition = transform.position;
		Quaternion prevRotation = transform.rotation;

		foreach(AnimationClip clip in anim.runtimeAnimatorController.animationClips)
		{
			if(clip.name == clipName)
			{
				clip.SampleAnimation(gameObject, 0f);
				CopyBoneTransforms(boneTransforms);
			}
		}

		transform.position = prevPosition;
		transform.rotation = prevRotation;
	}

	private void SetBodyParts()
	{
		ZombieHitBox[] hitBoxes = GetComponentsInChildren<ZombieHitBox>();
		foreach (ZombieHitBox hitBox in hitBoxes)
		{
			BodyPart bodyPart = new BodyPart();
			bodyPart.zombieHitBox = hitBox;
			bodyPart.rb = hitBox.GetComponent<Rigidbody>();
			bodyPart.col = hitBox.GetComponent<Collider>();
			bodyParts[(int)hitBox.BodyType] = bodyPart;
		}
	}

	public void SetRbKinematic(bool value)
	{
		foreach(BodyPart bodyPart in bodyParts)
		{
			bodyPart.rb.isKinematic = !value;
			bodyPart.rb.detectCollisions = value;
			bodyPart.col.isTrigger = !value;
		}
	}

	public override void Spawned()
	{
		Agent.enabled = true;
		int cnt = 0;
		foreach (Transform child in skins)
		{
			if (cnt == SkinIdx)
				child.gameObject.SetActive(true);
			else
				child.gameObject.SetActive(false);
			cnt++;
		}

		//HitboxRoot hitboxRoot = GetComponent<HitboxRoot>();
		//foreach(Hitbox hitbox in hitboxRoot.Hitboxes)
		//{
		//	hitbox.gameObject.SetActive(true);
		//}

		//Hitbox[] hits = GetComponentsInChildren<Hitbox>();
		//print(hits.Length);
		//foreach (Hitbox hit in hits)
		//{
		//	print(hit.name);
		//	hit.gameObject.SetActive(true);
		//}
	}

	public override void FixedUpdateNetwork()
	{
		Position = transform.position;
		Rotation = transform.rotation;
	}

	public override void Render()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine($"현재 상태: {stateMachine.curStateStr}");
		sb.AppendLine($"SpeedX : {anim.GetFloat("SpeedX"):#.##}");
		sb.AppendLine($"SpeedY : {anim.GetFloat("SpeedY"):#.##}");
		sb.AppendLine($"curPos : {transform.position}");
		sb.AppendLine($"Pos: {Position}");
		sb.AppendLine($"PosDiff: {(transform.position - Position).sqrMagnitude.ToString("F4")}");

		curStateText.text = sb.ToString();

		if (Object.IsProxy)
		{
			if ((transform.position - Position).sqrMagnitude > Mathf.Lerp(0.01f, 1f, anim.GetFloat("SpeedY") * 0.2f))
			{
				//debugCapsule.transform.position = transform.position;
				//debugCapsule.SetActive(true);
				Agent.enabled = false;
				transform.position = Position;
				Agent.enabled = true;
			}
			transform.rotation = Rotation;
		}
	}

	public void RagdollChanged()
	{
		if (IsRagdoll == true)
		{
			stateMachine.ChangeState(State.RagdollEnter);
		}
		else
		{
			stateMachine.ChangeState(State.RagdollExit);
		}
	}

	public float GetHitBodyFloat(ZombieBody zombieBody)
	{
		float hitBodyType = 0f;
		switch (zombieBody)
		{
			case ZombieBody.Head:
			case ZombieBody.MiddleSpine:
			case ZombieBody.Pelvis:
				hitBodyType = 0f;
				break;
			case ZombieBody.LeftArm:
			case ZombieBody.LeftElbow:
				hitBodyType = 1f;
				break;
			case ZombieBody.RightArm:
			case ZombieBody.RightElbow:
				hitBodyType = 2f;
				break;
			case ZombieBody.LeftHips:
			case ZombieBody.LeftKnee:
				hitBodyType = 3f;
				break;
			case ZombieBody.RightHips:
			case ZombieBody.RightKnee:
				hitBodyType = 4f;
				break;
			default:
				Debug.LogError($"ZombieBody 예외 처리 안됨: {zombieBody}");
				break;
		}
		return hitBodyType;
	}

	public void Init(NetworkObject target)
	{
		Target = target;
		TraceSpeed = Random.Range(1, 4);

		int skinCnt = skins.childCount;
		SkinIdx = Random.Range(0, skinCnt);
	}

	public void SetAnimBool(string name, bool value)
	{
		anim.SetBool(name, value);
	}

	public void SetAnimFloat(string name, float value)
	{
		anim.SetFloat(name, value);
	}

	public void SetAnimFloat(string name, float value, float dampTime, float? deltaTime = null)
	{
		if(deltaTime.HasValue == false)
		{
			deltaTime = Runner.DeltaTime;
		}
		anim.SetFloat(name, value, dampTime, deltaTime.Value);
	}

	public void SetAnimTrigger(string name)
	{
		anim.SetTrigger(name);
	}

	public bool IsAnimName(string name, int layer = 0)
	{
		return anim.GetCurrentAnimatorStateInfo(layer).IsName(name);
	}

	//private void OnDrawGizmos()
	//{
	//	Gizmos.color = Color.yellow;
	//	Gizmos.DrawSphere(transform.position + Vector3.up * 0.3f + transform.forward * 0.1f, 0.05f);
	//}

	public State DecideState()
	{
		if (Object.IsProxy)
		{
			return State.Idle;
		}

		if(anim.GetBool("Crawl") == true)
		{
			if(Target == null)
			{
				return State.CrawlIdle;
			}
			else
			{
				return State.CrawlIdle;
				//공격으로 전환
			}
		}


		if(Target != null)
		{
			return State.Trace;
		}
		else
		{
			return State.Idle;
		}
	}

	public void ApplyDamage(ZombieHitBox zombieHitBox, Vector3 velocity, int damage)
	{
		if (Object.IsProxy) return;

		float hitBodyFloat = GetHitBodyFloat(zombieHitBox.BodyType);

		//상체에 맞으면
		if(hitBodyFloat < 2.9f)
		{
			if(true)//Vector3.Dot(velocity, transform.forward) > 0)
			{
				RagdollVelocity = velocity;
				RagdollBody = zombieHitBox.BodyType;
				IsRagdoll = true;
				stateMachine.ChangeState(State.RagdollEnter);
				return;
			}
		}

		SetAnimFloat("HitBodyType", hitBodyFloat);
		SetAnimTrigger("Hit");

		AnimWaitStruct = new AnimWaitStruct("StandHit", "Idle",
			updateAction: () => SetAnimFloat("SpeedY", 0f, 0.1f));
		stateMachine.ChangeState(State.AnimWait);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(LastFootPos, 0.1f);
	}
}