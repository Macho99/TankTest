using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public enum RagdollState { Animate, Ragdoll, FaceUpStand, FaceDownStand, FaceUpCrawl, FaceDownCrawl };

public class Zombie : ZombieBase
{
	public enum State { Idle, Trace, AnimWait, Wait, CrawlIdle, CrawlTrace, RagdollEnter, RagdollExit, Eat, Die }

	[SerializeField] Transform skins;
	[SerializeField] float fallAsleepThreshold = 0.2f;
	[SerializeField] TextMeshProUGUI curStateText;

	TickTimer meatFindTimer;

	public Transform[] Bones { get; private set; }
	public BoneTransform[] RagdollBoneTransforms { get; private set; }
	public static Dictionary<int, BoneTransform[]> BoneTransDict { get; private set; }
	//public static BoneTransform[] FaceUpBoneTransforms { get; private set; }
	//public static BoneTransform[] FaceDownBoneTranforms { get; private set; }
	public float FallAsleepThreshold { get { return fallAsleepThreshold; } }
	public LayerMask FallAsleepMask { get; set; }
	public LayerMask MeatMask { get; set; }

	private float maxSpeed;
	public float TraceSpeed { get 
		{ return Mathf.Max(maxSpeed * (((float) CurLegHp) / MaxLegHp), 1f); }
	}

	public int SkinIdx { get; private set; }
	public Tick LastHitTick { get; private set; }
	public int MaxLegHp { get; private set; } = 100;
	public int CurLegHp { get; private set; }

	[Networked, OnChangedRender(nameof(RagdollChanged))] public RagdollState CurRagdollState { get; set; }
	[Networked, OnChangedRender(nameof(RagdollStart))] public int RagdollCnt { get; set; }

	protected override void Awake()
	{
		base.Awake();

		CurLegHp = MaxLegHp;
		MeatMask = LayerMask.GetMask("Meat");
		FallAsleepMask = LayerMask.GetMask("FallAsleepObject");

		Bones = Hips.GetComponentsInChildren<Transform>();
		RagdollBoneTransforms = new BoneTransform[Bones.Length];
		if(BoneTransDict == null)
		{
			InitBoneTransDict();
		}

		stateMachine.AddState(State.Idle, new ZombieIdle(this));
		stateMachine.AddState(State.Trace, new ZombieTrace(this));
		stateMachine.AddState(State.AnimWait, new ZombieAnimWait(this));
		stateMachine.AddState(State.CrawlIdle, new ZombieCrawlIdle(this));
		stateMachine.AddState(State.CrawlTrace, new ZombieCrawlTrace(this));
		stateMachine.AddState(State.Wait, new ZombieWait(this));
		stateMachine.AddState(State.RagdollEnter, new ZombieRagdollEnter(this));
		stateMachine.AddState(State.RagdollExit, new ZombieRagdollExit(this));
		stateMachine.AddState(State.Die, new ZombieDie(this));

		SetAnimBool("Crawl", true);
		AnimWaitStruct = new AnimWaitStruct("Spawn", State.CrawlIdle.ToString());

		stateMachine.InitState(State.AnimWait);
	}

	public override void Spawned()
	{
		base.Spawned();

		Random.InitState((int)Object.Id.Raw * Runner.SessionInfo.Name.GetHashCode());
		maxSpeed = Random.Range(1, 4);

		anim.SetFloat("WalkShifter", Random.Range(0, 5));
		anim.SetFloat("IdleShifter", Random.Range(0, 3));
		anim.SetFloat("RunShifter", Random.Range(0, 2));
		anim.SetFloat("SprintShifter", Random.Range(0, 2));

		int skinCnt = skins.childCount;
		SkinIdx = Random.Range(0, skinCnt);

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
		base.FixedUpdateNetwork();

		FindMeat();
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

		agent.ResetPath();
		Target = overlapCols[0].transform;
		CurTargetType = TargetType.Meat;
	}


	public override void Render()
	{
		string curState = stateMachine.curStateStr;
		StringBuilder sb = new StringBuilder();
		string printState = curState.Equals("AnimWait") ? $"{prevState} -> AnimWait({WaitName}) -> {NextState}" : curState;
		sb.AppendLine($"현재 상태: {printState}");
		sb.AppendLine($"마지막 타겟 발견시간: {((LastPlayerFindTick - Runner.Tick) / (float) Runner.TickRate).ToString("F1")}");
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
		if (curState.Equals("AnimWait") == false)
			prevState = curState;

		curStateText.text = sb.ToString();


		if (CurRagdollState != RagdollState.Animate) return;

		base.Render();
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

	protected void RagdollStart()
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

	private void StartRagdoll()
	{
		LastPlayerFindTick = Runner.Tick + Runner.TickRate * 10;
		CurRagdollState = RagdollState.Ragdoll;
		RagdollCnt++;
	}

	#endregion

	public override string DecideState()
	{
		if (Object.IsProxy)
			return State.Idle.ToString();

		if(anim.GetBool("Crawl") == true)
		{
			return State.CrawlIdle.ToString();
		}

		return State.Idle.ToString();
	}

	public override void ApplyDamage(Transform source, ZombieHitBox zombieHitBox, Vector3 velocity, int damage)
	{
		base.ApplyDamage(source, zombieHitBox, velocity, damage);
		if (Object.IsProxy) return;

		Target = source;
		CurTargetType = TargetType.Player;
		LastPlayerFindTick = Runner.Tick;

		if (CurHp <= 0)
		{
			StartRagdoll();
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
			StartRagdoll();
			return;
		}

        //상체에 맞으면
        if (hitBodyFloat < 2.9f)
		{
			//뒤쪽 상체에 맞으면 래그돌
			if(Vector3.Dot(velocity, transform.forward) > 0)
			{
				StartRagdoll();
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
			StartRagdoll();
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
				updateAction: () =>
				{
					SetAnimFloat("SpeedX", 0f, 1f);
					SetAnimFloat("SpeedY", 0f, 1f);
				});
			stateMachine.ChangeState(State.AnimWait);
		}
		else
		{
			SetAnimBool("Crawl", true);
			SetAnimFloat("FallAsleep", 0f);

			AnimWaitStruct = new AnimWaitStruct("Fall", State.CrawlIdle.ToString(),
				updateAction: () =>
				{
					SetAnimFloat("SpeedX", 0f, 1f);
					SetAnimFloat("SpeedY", 0f, 1f);
				});
			stateMachine.ChangeState(State.AnimWait);
		}
	}

	public override void Heal(int amount, bool healLeg = true)
	{
		base.Heal(amount, healLeg);
		if(healLeg == true)
		{
			CurLegHp += amount;
			CurLegHp = Mathf.Min(CurLegHp, MaxLegHp);
		}
	}

	public void Attack(int damage)
	{
		Heal(20);
	}
}