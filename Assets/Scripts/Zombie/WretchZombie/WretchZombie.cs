using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct PoisonVFXData : INetworkStruct
{
	public int fireTick;
	public int finishTick;

	public Vector3 startPosition;
	public Vector3 velocity;
	public Vector3 explosionPosition;
}

public class WretchZombie : ZombieBase
{
	private struct PoisonAreaStruct
	{
		public Vector3 point;
		public TickTimer damageTimer;
		public TickTimer areaEndTimer;
	}

	public enum State { Idle, AnimWait, Trace, Hide, Die }

	[SerializeField] VFXAutoOff poisonFlyVFXPrefab;
	[SerializeField] float poisonCooltime = 20f;
	[SerializeField] float poisonRadius = 5f;
	[SerializeField] float poisonLiftTime = 10f;
	[SerializeField] float poisonGravity = -9.8f;
	[SerializeField] float poisonSpeed = 10f;
	const float maxPoisonFlyTime = 30f;

	Transform headTrans;
	Tick poisonExplosionTick;

	TickTimer hitReactionTimer;
	TickTimer poisonTimer;
	LayerMask poisonMask;
	Vector3 prevPoisonVel;
	Vector3 prevPoisonPos;
	bool isPoisonFly;
	int visualPoisonCnt;
	Coroutine poisonCoroutine;

	List<PoisonAreaStruct> poisonAreaList = new();

	public TickTimer PoisonTimer { get { return poisonTimer; } }

	[Networked] public PoisonVFXData CurPoisonVFXData { get; private set; }
	[Networked, OnChangedRender(nameof(PoisonRender))] public int NetPoisonCnt { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		poisonMask = LayerMask.GetMask("Player", "Vehicle", "Default", "Environment");
		headTrans = anim.GetBoneTransform(HumanBodyBones.Head);

		OnDie += () =>
		{
			SetAnimBool("Die", true);
			stateMachine.ChangeState(State.Die);
		};

		stateMachine.AddState(State.Idle, new WretchIdle(this));
		stateMachine.AddState(State.Trace, new WretchTrace(this));
		stateMachine.AddState(State.AnimWait, new ZombieAnimWait(this));
		stateMachine.AddState(State.Die, new ZombieBaseDie(this));

		stateMachine.InitState(State.Idle);
	}

	private void PoisonAreaAttack(Vector3 origin)
	{

	}

	public override void FixedUpdateNetwork()
	{
		base.FixedUpdateNetwork();

		print(CurPoisonVFXData.finishTick);
		print(CurPoisonVFXData.explosionPosition);


		for (int i = 0; i < poisonAreaList.Count; i++)
		{
			PoisonAreaStruct poisonArea = poisonAreaList[i];
			if (poisonArea.areaEndTimer.ExpiredOrNotRunning(Runner))
			{
				poisonAreaList.RemoveAt(i);
				i--;
				continue;
			}

			if (poisonArea.damageTimer.ExpiredOrNotRunning(Runner))
			{
				poisonArea.damageTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
				PoisonAreaAttack(poisonArea.point);
				poisonAreaList[i] = poisonArea;
			}
		}

		if (isPoisonFly == true)
		{
			float renderTime = Runner.LocalRenderTime;
			float startTime = CurPoisonVFXData.fireTick / Runner.TickRate;
			float flyTime = renderTime - startTime;
			Vector3 nextPoisonPos = CurPoisonVFXData.startPosition + (CurPoisonVFXData.velocity +
				(0.5f * flyTime * poisonGravity * Vector3.up)) * flyTime;

			Vector3 posDiff = nextPoisonPos - prevPoisonPos;
			
			Debug.DrawLine(prevPoisonPos, nextPoisonPos, Color.red);
			if(Physics.Raycast(prevPoisonPos, posDiff.normalized, out var hit, posDiff.magnitude, poisonMask))
			{
				PoisonVFXData data = CurPoisonVFXData;
				data.explosionPosition = hit.point;
				data.finishTick = Runner.Tick;
				CurPoisonVFXData = data;
				isPoisonFly = false;
			}
			else if (CurPoisonVFXData.fireTick / Runner.TickRate + maxPoisonFlyTime < Runner.LocalRenderTime)
			{
				PoisonVFXData data = CurPoisonVFXData;
				data.explosionPosition = nextPoisonPos;
				data.finishTick = Runner.Tick;
				CurPoisonVFXData = data;
				isPoisonFly = false;
			}

			prevPoisonPos = nextPoisonPos;
		}
	}

	public override void ApplyDamage(Transform source, ZombieHitBox zombieHitBox, Vector3 point, Vector3 force, int damage, bool playHitVFX = true)
	{
		base.ApplyDamage(source, zombieHitBox, point, force, damage, playHitVFX);

		if (IsDead == true)
			return;

		if (hitReactionTimer.ExpiredOrNotRunning(Runner) && damage > 500)
		{
			float hitShifter;
			Vector3 forceDir = force.normalized;
			if (Vector3.Dot(transform.forward, forceDir) > 0f)
			{
				hitShifter = -1;
			}
			else
			{
				hitShifter = 1;
			}
			SetAnimFloat("HitShifter", hitShifter);
			SetAnimTrigger("Hit");
			hitReactionTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
			AnimWaitStruct = new AnimWaitStruct("Hit", "Trace", updateAction: () =>
				{
					Decelerate();
					LookToward(-forceDir, 120f);
				});
			stateMachine.ChangeState(State.AnimWait);
		}
	}

	private void PoisonShot()
	{
		if (HasStateAuthority == false) return;

		if(isPoisonFly == true)
		{
			Debug.LogError("독이 발사중입니다");
			return;
		}

		poisonTimer = TickTimer.CreateFromSeconds(Runner, poisonCooltime);
		NetPoisonCnt++;
		CurPoisonVFXData = new PoisonVFXData()
		{
			fireTick = Runner.Tick,
			finishTick = 0,

			startPosition = anim.GetBoneTransform(HumanBodyBones.Head).position,
			velocity = transform.forward * 10f + transform.up * 5f,
		};
		print(CurPoisonVFXData.startPosition);
		print(CurPoisonVFXData.velocity);
		prevPoisonVel = CurPoisonVFXData.velocity;
		prevPoisonPos = CurPoisonVFXData.startPosition;
		isPoisonFly = true;
	}

	private void PoisonRender()
	{
		if (visualPoisonCnt == NetPoisonCnt) return;

		if(poisonCoroutine != null)
			StopCoroutine(poisonCoroutine);
		poisonCoroutine = StartCoroutine(CoPoison());
	}

	private IEnumerator CoPoison()
	{
		VFXAutoOff poisonFlyVFX = GameManager.Resource.Instantiate(poisonFlyVFXPrefab, 
			headTrans.position, headTrans.rotation, true);
		poisonFlyVFX.SetOffTime(maxPoisonFlyTime * 2f);

		while(Runner.Tick.Raw < CurPoisonVFXData.fireTick)
		{
			yield return null;
		}

		while (CurPoisonVFXData.finishTick == 0 || Runner.Tick.Raw < CurPoisonVFXData.finishTick)
		{
			float renderTime = Object.IsProxy ? Runner.RemoteRenderTime : Runner.LocalRenderTime;
			float startTime = CurPoisonVFXData.fireTick / Runner.TickRate;
			float flyTime = renderTime - startTime;
			Vector3 pos = CurPoisonVFXData.startPosition + (CurPoisonVFXData.velocity + 
				(0.5f * flyTime * poisonGravity * Vector3.up)) * flyTime;

			poisonFlyVFX.transform.position = pos;
			yield return null;
		}

		poisonFlyVFX.SetOffTime(3f);
	}

	public override string DecideState()
	{
		throw new NotImplementedException();
	}
}