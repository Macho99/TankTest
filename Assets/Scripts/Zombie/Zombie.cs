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

public enum RagdollState { Animate, Ragdoll, FaceUpStand, FaceDownStand, FaceUpCrawl, FaceDownCrawl };

public class Zombie : NetworkBehaviour
{
	public struct BodyPart
	{
		public ZombieHitBox zombieHitBox;
		public Rigidbody rb;
		public Collider col;
	}

	public enum State { Idle, Wander, Trace, AnimWait, Wait, CrawlIdle, RagdollEnter, RagdollExit }

	[SerializeField] Transform skins;
	[SerializeField] float fallAsleepThreshold = 0.2f;
	[SerializeField] TextMeshProUGUI curStateText;

	NavMeshAgent agent;
	NetworkStateMachine stateMachine;
	Animator anim;
	Rigidbody[] rbs;
	Collider[] cols;

	BodyPart[] bodyHitParts = new BodyPart[(int) ZombieBody.Size];

	public Transform[] Bones { get; private set; }
	public BoneTransform[] RagdollBoneTransforms { get; private set; }
	public static Dictionary<int, BoneTransform[]> BoneTransDict { get; private set; }
	//public static BoneTransform[] FaceUpBoneTransforms { get; private set; }
	//public static BoneTransform[] FaceDownBoneTranforms { get; private set; }
	public BodyPart[] BodyHitParts { get { return bodyHitParts; } }
	public Transform Hips { get; private set; }
	public Animator Anim { get { return anim; } }
	public float FallAsleepThreshold { get { return fallAsleepThreshold; } }
	public NavMeshAgent Agent { get { return agent; } }
	public LayerMask FallAsleepMask { get; set; }
	//	#region Variable For Specific State

	// AnimWait State
	public AnimWaitStruct? AnimWaitStruct { get; set; }
	//	#endregion

	public Transform Target { get; private set; }
	public float TraceSpeed { get; private set; }
	public int SkinIdx { get; private set; }
	public int CurLegHp { get; private set; } = -1;
	public int CurHp { get; private set; } = 300;

	[Networked] public Vector3 Position { get; set; }
	[Networked] public Quaternion Rotation { get; set; }
	[Networked, OnChangedRender(nameof(RagdollChanged))] public RagdollState CurRagdollState { get; set; }
	[Networked, OnChangedRender(nameof(RagdollStart))] public int RagdollCnt { get; set; }
	[Networked] public ZombieBody RagdollBody { get; private set; }
	[Networked] public Vector3 RagdollVelocity { get; private set; }

	private void Awake()
	{
		FallAsleepMask = LayerMask.GetMask("FallAsleepObject");
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();

		Hips = anim.GetBoneTransform(HumanBodyBones.Hips);
		Bones = Hips.GetComponentsInChildren<Transform>();
		RagdollBoneTransforms = new BoneTransform[Bones.Length];
		if(BoneTransDict == null)
		{
			InitBoneTransDict();
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
		SetRbKinematic(true);
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

	private void InitBoneTransDict()
	{
		Vector3 prevPosition = transform.position;
		Quaternion prevRotation = transform.rotation;

		BoneTransDict = new();

		foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
		{
			BoneTransform[] boneTransforms = new BoneTransform[Bones.Length];
			clip.SampleAnimation(gameObject, 0f);
			CopyBoneTransforms(boneTransforms);
			BoneTransDict.Add(clip.name.GetHashCode(), boneTransforms);
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
			bodyHitParts[(int)hitBox.BodyType] = bodyPart;
		}
	}

	public void SetRbKinematic(bool value)
	{
		foreach(BodyPart bodyPart in bodyHitParts)
		{
			bodyPart.rb.isKinematic = value;
			bodyPart.rb.detectCollisions = !value;
			bodyPart.col.isTrigger = value;
		}
	}

	public override void Spawned()
	{
		Random.InitState((int)Object.Id.Raw * Runner.SessionInfo.Name.GetHashCode());
		TraceSpeed = Random.Range(1, 4);

		anim.SetFloat("IdleShifter", Random.Range(0, 3));
		anim.SetFloat("WalkShifter", Random.Range(0, 5));
		anim.SetFloat("RunShifter", Random.Range(0, 2));
		anim.SetFloat("SprintShifter", Random.Range(0, 2));

		int skinCnt = skins.childCount;
		SkinIdx = Random.Range(0, skinCnt);

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
		sb.AppendLine($"CurRagdollState: {CurRagdollState}");
		sb.AppendLine($"SpeedX : {anim.GetFloat("SpeedX"):#.##}");
		sb.AppendLine($"SpeedY : {anim.GetFloat("SpeedY"):#.##}");
		sb.AppendLine($"PosDiff: {(transform.position - Position).sqrMagnitude.ToString("F4")}");

		curStateText.text = sb.ToString();

		if (Object.IsProxy)
		{
			if (CurRagdollState != RagdollState.Animate && CurRagdollState != RagdollState.Ragdoll) return;

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

	public void RagdollStart()
	{
		stateMachine.ChangeState(State.RagdollEnter);
	}

	public void RagdollChanged()
	{
		switch (CurRagdollState)
		{
			case RagdollState.FaceDownStand:
			case RagdollState.FaceUpStand:
			case RagdollState.FaceDownCrawl:
			case RagdollState.FaceUpCrawl:
				stateMachine.ChangeState(State.RagdollExit);
				break;
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

	public State DecideState()
	{
		if (Object.IsProxy)
			return State.Idle;

		if(anim.GetBool("Crawl") == true)
		{
			if(Target == null)
				return State.CrawlIdle;
			else
				return State.CrawlIdle;
			//공격으로 전환
		}


		if(Target != null)
			return State.Trace;
		else
			return State.Idle;
	}

	public void ApplyDamage(Transform source, ZombieHitBox zombieHitBox, Vector3 velocity, int damage)
	{
		if (Object.IsProxy) return;

		Target = source;
		float hitBodyFloat = GetHitBodyFloat(zombieHitBox.BodyType);

		//이미 래그돌 중이면
		if(CurRagdollState != RagdollState.Animate)
		{
			StartRagdoll(velocity, zombieHitBox.BodyType);
			return;
		}

        //상체에 맞으면
        if (hitBodyFloat < 2.9f)
		{
			if(Vector3.Dot(velocity, transform.forward) > 0)
			{
				StartRagdoll(velocity, zombieHitBox.BodyType);
				return;
			}

			Vector3 lookDir = -velocity;
			lookDir.y = 0f;
			transform.rotation = Quaternion.LookRotation(lookDir);
		}

		SetAnimFloat("HitBodyType", hitBodyFloat);
		SetAnimTrigger("Hit");

		AnimWaitStruct = new AnimWaitStruct("StandHit", "Idle",
			updateAction: () => SetAnimFloat("SpeedY", 0f, 0.1f));
		stateMachine.ChangeState(State.AnimWait);
	}

	private void StartRagdoll(Vector3 velocity, ZombieBody zombieBody)
	{
		RagdollVelocity = velocity;
		RagdollBody = zombieBody;
		CurRagdollState = RagdollState.Ragdoll;
		RagdollCnt++;
	}
}