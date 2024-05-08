using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class WretchZombie : ZombieBase
{
	public enum State { Idle, AnimWait, Trace, Hide, Die }
	[SerializeField] float poisonCooltime = 20f;

	TickTimer hitReactionTimer;
	TickTimer poisonTimer;

	public TickTimer PoisonTimer { get { return poisonTimer; } }

	protected override void Awake()
	{
		base.Awake();
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

	}

	public override string DecideState()
	{
		throw new NotImplementedException();
	}
}