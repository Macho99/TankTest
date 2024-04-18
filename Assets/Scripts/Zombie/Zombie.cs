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

	public enum TargetType { None, Meat, Player }

	public enum State { Idle, Wander, Trace, AnimWait, Wait, CrawlIdle, CrawlTrace, RagdollEnter, RagdollExit, Eat, Die }

	[SerializeField] Transform skins;
	[SerializeField] float fallAsleepThreshold = 0.2f;
	[SerializeField] TextMeshProUGUI curStateText;
	[SerializeField] float viewAngle = 60f;
	[SerializeField] float playerLostTime = 5f;
	[SerializeField] GameObject headBloodVFX;
	[SerializeField] GameObject bodyBloodVFX;

	TickTimer destinationTimer;
	TickTimer playerFindTimer;
	TickTimer meatFindTimer;
	NavMeshAgent agent;
	NetworkStateMachine stateMachine;
	Animator anim;
	Collider[] overlapCols = new Collider[5];

	BodyPart[] bodyHitParts = new BodyPart[(int)ZombieBody.Size];

	public Transform[] Bones { get; private set; }
	public BoneTransform[] RagdollBoneTransforms { get; private set; }
	public static Dictionary<int, BoneTransform[]> BoneTransDict { get; private set; }
	//public static BoneTransform[] FaceUpBoneTransforms { get; private set; }
	//public static BoneTransform[] FaceDownBoneTranforms { get; private set; }
	public BodyPart[] BodyHitParts { get { return bodyHitParts; } }
	public Transform Hips { get; private set; }
	public Transform Head { get; private set; }
	public Animator Anim { get { return anim; } }
	public float FallAsleepThreshold { get { return fallAsleepThreshold; } }
	public NavMeshAgent Agent { get { return agent; } }
	public LayerMask FallAsleepMask { get; set; }
	public LayerMask PlayerMask { get; set; }
	public LayerMask MeatMask { get; set; }
	public int PlayerLayer { get; set; }
	//	#region Variable For Specific State

	// AnimWait State
	public AnimWaitStruct? AnimWaitStruct { get; set; }
	//	#endregion

	public TargetType CurTargetType { get; set; }
	public Transform Target { get; set; }

	private float maxSpeed;
	public float TraceSpeed { get 
		{ return Mathf.Max(maxSpeed * (((float) CurLegHp) / MaxLegHp), 1f); }
	}

	public int SkinIdx { get; private set; }
	public Tick LastHitTick { get; private set; }
	public Tick LastPlayerFindTick { get; private set; }
	public int MaxLegHp { get; private set; } = 100;
	public int CurLegHp { get; private set; }
	public int CurHp { get; private set; }
	public int MaxHP { get; private set; } = 300;

	[Networked] public Vector3 Position { get; set; }
	[Networked] public Quaternion Rotation { get; set; }
	[Networked, OnChangedRender(nameof(RagdollChanged))] public RagdollState CurRagdollState { get; set; }
	[Networked, OnChangedRender(nameof(RagdollStart))] public int RagdollCnt { get; set; }
	[Networked] public ZombieBody RagdollBody { get; private set; }
	[Networked] public Vector3 RagdollVelocity { get; private set; }

	private void Awake()
	{
		CurLegHp = MaxLegHp;
		CurHp = MaxHP;
		MeatMask = LayerMask.GetMask("Meat");
		PlayerMask = LayerMask.GetMask("Player");
		PlayerLayer = LayerMask.NameToLayer("Player");
		FallAsleepMask = LayerMask.GetMask("FallAsleepObject");
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();

		Hips = anim.GetBoneTransform(HumanBodyBones.Hips);
		Head = anim.GetBoneTransform(HumanBodyBones.Head);
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
		stateMachine.AddState(State.CrawlTrace, new ZombieCrawlTrace(this));
		stateMachine.AddState(State.Wait, new ZombieWait(this));
		stateMachine.AddState(State.RagdollEnter, new ZombieRagdollEnter(this));
		stateMachine.AddState(State.RagdollExit, new ZombieRagdollExit(this));
		stateMachine.AddState(State.Die, new ZombieDie(this));

		stateMachine.InitState(State.Idle);

		SetBodyParts();
		SetRbKinematic(true);
	}

	public override void Spawned()
	{
		Random.InitState((int)Object.Id.Raw * Runner.SessionInfo.Name.GetHashCode());
		maxSpeed = Random.Range(1, 4);

		anim.SetFloat("WalkShifter", Random.Range(0, 5));
		anim.SetFloat("IdleShifter", Random.Range(0, 3));
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

		FindPlayer();
		FindMeat();
		TargetManage();
	}

	private void FindPlayer()
	{
		if (CurTargetType == TargetType.Player) return;
		if (playerFindTimer.ExpiredOrNotRunning(Runner) == false) return;

		int result = Physics.OverlapSphereNonAlloc(transform.position, 10f, overlapCols, PlayerMask);

		if(result == 0)
		{
			playerFindTimer = TickTimer.CreateFromSeconds(Runner, 1f);
			return;
		}

		Transform findPlayer = overlapCols[0].transform;
		Vector3 toPlayerVec = ((findPlayer.position + Vector3.up) - Head.position);
		if (Physics.Raycast(Head.position, toPlayerVec.normalized, toPlayerVec.magnitude, LayerMask.GetMask("Default")) == false)
		{
			LastPlayerFindTick = Runner.Tick;
			Target = overlapCols[0].transform;
			CurTargetType = TargetType.Player;
		}
		playerFindTimer = TickTimer.CreateFromSeconds(Runner, 0.1f);
	}

	private void FindMeat()
	{
		if (agent.hasPath || CurTargetType == TargetType.Player) return;
		if (meatFindTimer.ExpiredOrNotRunning(Runner) == false) return;
		if (CurHp == MaxHP) return;

		meatFindTimer = TickTimer.CreateFromSeconds(Runner, 5f);

		int result = Physics.OverlapSphereNonAlloc(transform.position, 15f, overlapCols, MeatMask);

		if (result == 0)
			return;

		Target = overlapCols[0].transform;
		CurTargetType = TargetType.Meat;
	}

	private void TargetManage()
	{
		if (Target == null) return;

		if (destinationTimer.ExpiredOrNotRunning(Runner))
		{
			destinationTimer = TickTimer.CreateFromSeconds(Runner, 1f);
			if (Target != null && agent.enabled == true)
				agent.SetDestination(Target.position);
		}

		if (CurTargetType != TargetType.Player) return;

		Vector3 toPlayerVec = ((Target.position + Vector3.up) - Head.position);
		if (Physics.Raycast(Head.position, toPlayerVec.normalized, toPlayerVec.magnitude, LayerMask.GetMask("Default")) == false)
		{
			LastPlayerFindTick = Runner.Tick;
		}

		if (LastPlayerFindTick + playerLostTime * Runner.TickRate < Runner.Tick)
		{
			Target = null;
			CurTargetType = TargetType.None;
		}
	}

	string prevState;
	public override void Render()
	{
		string curState = stateMachine.curStateStr;
		StringBuilder sb = new StringBuilder();
		string printState = curState.Equals("AnimWait") ? $"{prevState} -> AnimWait" : curState;
		sb.AppendLine($"현재 상태: {printState}");
		sb.AppendLine($"LastPlayerFindTick: {LastPlayerFindTick}");
		sb.AppendLine($"Target: {CurTargetType}");
		sb.Append($"CurHP: ");
		for (int i = 0; i < CurHp; i += 50)
		{
			sb.Append("■");
		}
		sb.Append('\n');
		sb.AppendLine($"SpeedX : {anim.GetFloat("SpeedX"):#.##}");
		sb.AppendLine($"SpeedY : {anim.GetFloat("SpeedY"):#.##}");
		sb.AppendLine($"PosDiff: {(transform.position - Position).sqrMagnitude.ToString("F4")}");
		if(curState.Equals("AnimWait") == false)
			prevState = curState;

		curStateText.text = sb.ToString();

		if (Object.IsProxy)
		{
			if (CurRagdollState != RagdollState.Animate) return;

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
	#region RagdollMethod
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
			int hashCode = clip.name.GetHashCode();
			if (BoneTransDict.ContainsKey(hashCode) == false)
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
		foreach (BodyPart bodyPart in bodyHitParts)
		{
			bodyPart.rb.isKinematic = value;
			bodyPart.rb.detectCollisions = !value;
			bodyPart.col.isTrigger = value;
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

	private void StartRagdoll(Vector3 velocity, ZombieBody zombieBody)
	{
		RagdollVelocity = velocity;
		RagdollBody = zombieBody;
		CurRagdollState = RagdollState.Ragdoll;
		RagdollCnt++;
	}

	#endregion

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

		SetAnimFloat("SpeedY", 0f);
		Target = source;
		CurTargetType = TargetType.Player;
		LastPlayerFindTick = Runner.Tick;

		GameObject vfx = zombieHitBox.BodyType == ZombieBody.Head ? headBloodVFX : bodyBloodVFX;
		Instantiate(vfx, zombieHitBox.transform.position, Quaternion.LookRotation(-velocity));

		CurHp -= damage;
		if (CurHp <= 0)
		{
			CurHp = 0;
			StartRagdoll(velocity, zombieHitBox.BodyType);
		}

		float hitBodyFloat = GetHitBodyFloat(zombieHitBox.BodyType);

		//하체에 맞으면 하체 체력 감소
		if (2.9f < hitBodyFloat)
		{
			CurLegHp -= damage;
		}

		//이미 래그돌 중이면 한번 더 래그돌
		//또는 Crawl 상태면 래그돌
		if(CurRagdollState != RagdollState.Animate || anim.GetBool("Crawl") == true)
		{
			StartRagdoll(velocity, zombieHitBox.BodyType);
			return;
		}

        //상체에 맞으면
        if (hitBodyFloat < 2.9f)
		{
			//뒤쪽 상체에 맞으면 래그돌
			if(Vector3.Dot(velocity, transform.forward) > 0)
			{
				StartRagdoll(velocity, zombieHitBox.BodyType);
				return;
			}

			//정면 상체에 맞으면 맞은 쪽으로 회전
			Vector3 lookDir = -velocity;
			lookDir.y = 0f;
			transform.rotation = Quaternion.LookRotation(lookDir);
		}

		// 1초 이내에 다시 맞으면
		if (LastHitTick + 1 * Runner.TickRate > Runner.Tick)
		{
			StartRagdoll(velocity, zombieHitBox.BodyType);
			return;
		}

		LastHitTick = Runner.Tick;

		//////////////////////////
		// 밑으로 래그돌이 아닌 상태
		//////////////////////////


		//하체 체력이 충분하면 부위별 피격 애니메이션 재생
		if (CurLegHp > 0)
		{
			SetAnimFloat("HitBodyType", hitBodyFloat);
			SetAnimTrigger("Hit");

			AnimWaitStruct = new AnimWaitStruct("StandHit", "Idle",
				updateAction: () => SetAnimFloat("SpeedY", 0f, 0.1f));
			stateMachine.ChangeState(State.AnimWait);
		}
		else
		{
			SetAnimBool("Crawl", true);
			SetAnimFloat("FallAsleep", 0f);

			AnimWaitStruct = new AnimWaitStruct("Fall", State.CrawlIdle.ToString(), 
				updateAction: () => SetAnimFloat("SpeedY", 0f, 0.1f));
			stateMachine.ChangeState(State.AnimWait);
		}
	}

	public void Heal(int amount, bool healLeg = true)
	{
		CurHp += amount;
		CurHp = Mathf.Clamp(CurHp, 0, MaxHP);

		if (healLeg == true)
		{
			CurLegHp += amount;
			CurLegHp = Mathf.Clamp(CurLegHp, 0, MaxLegHp);
		}
	}

	//public void OnTargetTriggerStay(Transform target)
	//{
	//	if (Object == null) return;
	//	if (Object.IsProxy) return;

	//	if (Target == null)
	//	{
	//		Vector3 targetDir = ((target.position + Vector3.up) - Head.position).normalized;
	//		if (Vector3.Dot(targetDir, Head.forward) > Mathf.Cos(viewAngle * Mathf.Deg2Rad))
	//		{
	//			if(Physics.Raycast(Head.position, targetDir, targetDir.magnitude, LayerMask.GetMask("Default")) == false)
	//			{
	//				Target = target;
	//			}
	//		}
	//	}
	//}

	public void Attack(int damage)
	{
		Heal(20);
	}
}