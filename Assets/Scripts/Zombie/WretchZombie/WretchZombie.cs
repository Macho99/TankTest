using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

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

	[SerializeField] FXAutoOff poisonFlyVFXPrefab;
	[SerializeField] FXAutoOff poisonExplosionPrefab;
	[SerializeField] PoisonArea poisonAreaPrefab;
	[SerializeField] float poisonCooltime = 20f;
	[SerializeField] float poisonRadius = 5f;
	[SerializeField] float poisonLiftTime = 10f;
	[SerializeField] float poisonGravity = -9.8f;
	[SerializeField] float poisonSpeed = 10f;
	const float maxPoisonFlyTime = 30f;

	Transform headTrans;

	TickTimer hitReactionTimer;
	TickTimer poisonAreaHitTimer;
	TickTimer poisonTimer;
	LayerMask poisonMask;
	Vector3 prevPoisonVel;
	Vector3 prevPoisonPos;
	bool isPoisonFly;
	int visualPoisonCnt;
	Coroutine poisonCoroutine;

	List<GameObject> poisonHitList = new();

	public Transform HeadTrans { get { return headTrans; } }
	public Vector3 PoisonGravity { get { return Vector3.up * poisonGravity; } }
	public Vector3 PoisonVelocity { get; set; }
	public float PoisonSpeed {  get { return poisonSpeed; } }
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
		stateMachine.AddState(State.Die, new ZombieBaseDie(this, 30f));

		stateMachine.InitState(State.Idle);
	}

	public override void FixedUpdateNetwork()
	{
		base.FixedUpdateNetwork();

		if(poisonHitList.Count > 0 && poisonAreaHitTimer.ExpiredOrNotRunning(Runner))
		{
			poisonAreaHitTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
			foreach(GameObject obj in poisonHitList)
			{
				IHittable hittable = obj.GetComponent<IHittable>();
				if(hittable == null)
					continue;
				hittable.ApplyDamage(transform, obj.transform.position, Vector3.zero, 5);
			}

			poisonHitList.Clear();
		}

		//for (int i = 0; i < poisonAreaList.Count; i++)
		//{
		//	PoisonAreaStruct poisonArea = poisonAreaList[i];
		//	if (poisonArea.areaEndTimer.ExpiredOrNotRunning(Runner))
		//	{
		//		poisonAreaList.RemoveAt(i);
		//		i--;
		//		continue;
		//	}

		//	if (poisonArea.damageTimer.ExpiredOrNotRunning(Runner))
		//	{
		//		poisonArea.damageTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
		//		PoisonAreaAttack(poisonArea.point);
		//		poisonAreaList[i] = poisonArea;
		//	}
		//}

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

		if (Object.IsProxy) return;

		if (IsDead == true)
			return;

		if (hitReactionTimer.ExpiredOrNotRunning(Runner) && damage > 100)
		{
			if (CurPoisonVFXData.fireTick == 0)
			{
				PoisonVFXData poisonVFXData = CurPoisonVFXData;
				poisonVFXData.fireTick = Runner.Tick;
				poisonVFXData.finishTick = Runner.Tick;
				poisonVFXData.explosionPosition = headTrans.position;
				CurPoisonVFXData = poisonVFXData;
			}

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
			PlaySound(ZombieSoundType.Hit);
			stateMachine.ChangeState(State.AnimWait);
		}
	}

	//Animation Event
	private void PoisonPrepare()
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
			fireTick = 0,
			finishTick = 0,
		};
	}

	//Animation Event
	private void PoisonShot()
	{
		if (HasStateAuthority == false) return;

		poisonTimer = TickTimer.CreateFromSeconds(Runner, poisonCooltime);
		PoisonVFXData poisonVFXData = CurPoisonVFXData;
		poisonVFXData.fireTick = Runner.Tick + 2;
		poisonVFXData.startPosition = anim.GetBoneTransform(HumanBodyBones.Head).position;
		poisonVFXData.velocity = PoisonVelocity;
		CurPoisonVFXData = poisonVFXData;

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
		FXAutoOff poisonFlyVFX = GameManager.Resource.Instantiate(poisonFlyVFXPrefab, 
			headTrans.position, headTrans.rotation, headTrans, true);
		poisonFlyVFX.SetOffTime(maxPoisonFlyTime * 2f);

		while(CurPoisonVFXData.fireTick == 0 || Runner.Tick.Raw < CurPoisonVFXData.fireTick)
		{
			yield return null;
		}

		poisonFlyVFX.transform.parent = null;

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

		poisonFlyVFX.SetOfftimeWithElapsed(5f);
		GameManager.Resource.Instantiate(poisonExplosionPrefab, 
			CurPoisonVFXData.explosionPosition, Random.rotation, true);

		PoisonArea poisonArea = GameManager.Resource.Instantiate(poisonAreaPrefab, 
			CurPoisonVFXData.explosionPosition, Quaternion.identity, true);
		if (HasStateAuthority)
		{
			poisonArea.SetOwner(this);
		}
	}

	public void AddPosionHit(GameObject obj)
	{
		if(poisonHitList.Contains(obj) == false)
		{
			poisonHitList.Add(obj);
		}
	}

	public override string DecideState()
	{
		throw new NotImplementedException();
	}
}