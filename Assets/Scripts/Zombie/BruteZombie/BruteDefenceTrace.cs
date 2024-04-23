using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.Rendering.DebugUI;

public class BruteDefenceTrace : BruteZombieState
{
	const float speed = 1f;
	const float rotateSpeed = 60f;
	const float defenceTime = 5f;

	TickTimer defenceTimer;

	public BruteDefenceTrace(BruteZombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		owner.Shield?.SetEnable(true);
		owner.SetAnimInt("Defence", 1);
		owner.OnHit += ResetTimer;
		owner.OnStun += StunCallback;
		ResetTimer();
	}

	public override void Exit()
	{
		owner.Shield?.SetEnable(false);
		owner.OnHit -= ResetTimer;
		owner.OnStun -= StunCallback;
	}

	private void StunCallback()
	{
		owner.SetAnimTrigger("DefenceExit");
	}

	public void ResetTimer()
	{
		defenceTimer = TickTimer.CreateFromSeconds(owner.Runner, defenceTime);
	}

	public override void FixedUpdateNetwork()
	{
		owner.LookTarget();
		owner.Trace(speed, rotateSpeed, 1f, 1f);
	}

	public override void SetUp()
	{
		owner.Shield.Init(this);
	}

	public override void Transition()
	{
		if (defenceTimer.ExpiredOrNotRunning(owner.Runner))
		{
			ChangeToTrace();
			return;
		}

		if (owner.Agent.hasPath && owner.Agent.remainingDistance < 5f)
		{
			ChangeToTrace();
			return;
		}
	}

	private void ChangeToTrace()
	{
		owner.SetAnimInt("Defence", 0);
		owner.AnimWaitStruct = new AnimWaitStruct("DefenceEnd", BruteZombie.State.Trace.ToString(), 1,
			updateAction: () =>
			{
				owner.LookTarget();
				owner.Trace(speed, rotateSpeed, 1f, 1f);
			});
		ChangeState(BruteZombie.State.AnimWait);
	}

	public void ShieldBreak()
	{
		owner.ShieldCnt--;
		if(owner.ShieldCnt == 0)
		{
			owner.ChangeToBerserk();
		}
        else
		{
			owner.SetAnimInt("Defence", 2);
			owner.AnimWaitStruct = new AnimWaitStruct("DefenceBreak", BruteZombie.State.Trace.ToString(), 1,
				updateAction: owner.Decelerate,
				exitAction: () => owner.SetAnimInt("Defence", 0));
			ChangeState(BruteZombie.State.AnimWait);
		}
	}

	public void Knockback()
	{
		owner.SetAnimFloat("ActionShifter", -1);
		owner.SetAnimTrigger("Hit");
		ChangeState(BruteZombie.State.DefenceKnockback);
	}
}