using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using static UnityEngine.UI.GridLayoutGroup;

public class BruteZombie : ZombieBase
{
	[SerializeField] Transform lookTarget;
	[SerializeField] Rig lookRig;
	[SerializeField] float lookSpeed = 1.5f;
	[SerializeField] Transform shieldHolder;

	public enum State { Idle, Trace, DefenceTrace, DefenceKnockback, Stun, BerserkTrace, Search, AnimWait, }

	public Action OnHit;
	public Action OnStun;

	public BruteShield Shield { get; private set; }
	public int ShieldCnt { get; set; } = 2;

	[Networked, OnChangedRender(nameof(BerserkRender))] public NetworkBool Berserk { get; set; }
	[Networked] public float LookWeight { get; set; }
	[Networked] public Vector3 LookPos { get; set; }

	protected override void Awake()
	{
		base.Awake();
		Shield = shieldHolder.GetComponentInChildren<BruteShield>();

		stateMachine.AddState(State.Idle, new BruteIdle(this));
		stateMachine.AddState(State.Trace, new BruteTrace(this));
		stateMachine.AddState(State.Search, new BruteSearch(this));
		stateMachine.AddState(State.DefenceTrace, new BruteDefenceTrace(this));
		stateMachine.AddState(State.DefenceKnockback, new BruteDefenceKnockback(this));
		stateMachine.AddState(State.BerserkTrace, new BruteBerserkTrace(this));
		stateMachine.AddState(State.AnimWait, new ZombieAnimWait(this));

		stateMachine.InitState(State.Idle);
	}

	public override void FixedUpdateNetwork()
	{
		base.FixedUpdateNetwork();
	}

	public override void Render()
	{
		base.Render();
		
		lookRig.weight = Mathf.Lerp(lookRig.weight, LookWeight, Runner.DeltaTime * lookSpeed);
		lookTarget.position = Vector3.Lerp(lookTarget.position, LookPos, Runner.DeltaTime * lookSpeed);
	}

	private void BerserkRender()
	{

	}

	public override string DecideState()
	{
		throw new NotImplementedException();
	}

	public void LookTarget()
	{
		if (Target != null)
		{
			LookPos = Target.position;
		}
		else
		{
			LookPos = transform.TransformPoint(Vector3.forward * 5f);
		}
	}

	public override void ApplyDamage(Transform source, ZombieHitBox zombieHitBox, 
		Vector3 velocity, int damage, bool playHitVFX = true)
	{
		base.ApplyDamage(source, zombieHitBox, velocity, damage, playHitVFX);

		Vector3 angleVelocity = velocity;
		angleVelocity.y = 0f;
		float angle = Vector3.SignedAngle(transform.forward, angleVelocity, transform.up);
		float sign = (angle >= 0f) ? 1f : -1f;
		float absAngle = Mathf.Abs(angle);

		//총알에 피격
		if (damage < 100)
		{
			float shifter;
			if(absAngle < 45f)
			{
				shifter = -1f;
			}
			else if (absAngle < 135f)
			{
				if(sign < 0f)
				{
					shifter = 1f;
				}
				else
				{
					shifter = 2f;
				}
			}
			else
			{
				shifter = 0f;
			}
			SetAnimFloat("LightHitShifter", shifter);
			SetAnimTrigger("LightHit");
		}
		//전차에 피격
		else
		{
			//뒤쪽에서 맞았으면 잠깐 스턴
			if(absAngle < 90f)
			{
				Stun(2f);
				return;
			}

			bool left;
			switch (zombieHitBox.BodyType)
			{
				case ZombieBody.LeftArm:
				case ZombieBody.LeftElbow:
				case ZombieBody.LeftHips:
				case ZombieBody.LeftKnee:
					left = true;
					break;
				case ZombieBody.RightArm:
				case ZombieBody.RightElbow:
				case ZombieBody.RightHips:
				case ZombieBody.RightKnee:
					left = false;
					break;
				default:
					if(sign < 0f)
						left = true;
					else
						left = false;
					break;
			}

			if(left == true)
			{
				SetAnimFloat("ActionShifter", -1f);
			}
			else
			{
				SetAnimFloat("ActionShifter", 1f);
			}

			SetAnimTrigger("Hit");
			if (Shield.Enabled == true)
			{
				stateMachine.ChangeState(State.DefenceKnockback);
			}
			else
			{
				State nextState;
				if(ShieldCnt > 0)
				{
					nextState = State.DefenceTrace;
				}
				else
				{
					nextState = State.BerserkTrace;
				}
				Shield.ResetHp();
				AnimWaitStruct = new AnimWaitStruct("Hit", nextState.ToString(),
					updateAction: Decelerate);
				stateMachine.ChangeState(State.AnimWait);
			}
		}

		OnHit?.Invoke();
	}

	bool isStun;
	bool exitTriggered;
	TickTimer durationTimer;
	public void Stun(float duration)
	{
		OnStun?.Invoke();
		if (isStun == true && exitTriggered == false)
		{
			float? nullableRemainTime = durationTimer.RemainingTime(Runner);
			float remainTime = nullableRemainTime.HasValue ? nullableRemainTime.Value : 0f;
			durationTimer = TickTimer.CreateFromSeconds(Runner, remainTime + duration);
			return;
		}

		isStun = true;
		exitTriggered = false;
		durationTimer = TickTimer.CreateFromSeconds(Runner, duration);
		SetAnimTrigger("Stun");
		AnimWaitStruct = new AnimWaitStruct("StunEnd", "Idle", startAnimName: "StunStart",
			startAction: () => LookWeight = 0f,
			updateAction: () =>
			{
				Decelerate(0.5f);
				if (exitTriggered == false && durationTimer.ExpiredOrNotRunning(Runner))
				{
					exitTriggered = true;
					SetAnimTrigger("Exit");
				}
			},
			exitAction: () => { isStun = false; LookWeight = 1f; });
		stateMachine.ChangeState(State.AnimWait);
	}

	public void BerserkMode()
	{
		SetAnimTrigger("DefenceExit");
		SetAnimTrigger("Down");
		Shield.Break();
		LookWeight = 0f;
		Berserk = true;
		AnimWaitStruct = new AnimWaitStruct("Rise", State.BerserkTrace.ToString(),
			updateAction: Decelerate, exitAction: () => LookWeight = 1f);
	}
}