using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
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

	public enum State { Idle, Trace, Search, AnimWait, }

	[Networked] public float LookWeight { get; set; }
	[Networked] public Vector3 LookPos { get; set; }

	protected override void Awake()
	{
		base.Awake();

		stateMachine.AddState(State.Idle, new BruteIdle(this));
		stateMachine.AddState(State.Trace, new BruteTrace(this));
		stateMachine.AddState(State.Search, new BruteSearch(this));
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

	public override void ApplyDamage(Transform source, ZombieHitBox zombieHitBox, Vector3 velocity, int damage)
	{
		base.ApplyDamage(source, zombieHitBox, velocity, damage);

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
				Stun(0.5f);
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
		}
	}

	bool isStun;
	bool exitTriggered;
	TickTimer durationTimer;
	public void Stun(float duration)
	{
		isStun = true;
		exitTriggered = false;
		durationTimer = TickTimer.CreateFromSeconds(Runner, duration);
		SetAnimTrigger("Stun");
		AnimWaitStruct = new AnimWaitStruct("StunEnd", "Idle",
		updateAction: () =>
		{
				SetAnimFloat("SpeedX", 0f, 0.5f);
				SetAnimFloat("SpeedY", 0f, 0.5f);
				if (exitTriggered == false && durationTimer.ExpiredOrNotRunning(Runner))
				{
					exitTriggered = true;
					SetAnimTrigger("Exit");
				}
			},
			exitAction: () => isStun = false);
		stateMachine.ChangeState(State.AnimWait);
	}
}