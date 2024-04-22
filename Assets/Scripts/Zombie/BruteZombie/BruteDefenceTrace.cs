using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		owner.SetAnimInt("Defence", 1);
		owner.OnHit += ResetTimer;
		owner.OnShieldBreak += ShieldBreak;
		ResetTimer();
	}

	public override void Exit()
	{
		owner.OnHit -= ResetTimer;
		owner.OnShieldBreak -= ShieldBreak;
	}

	private void ResetTimer()
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

	}

	public override void Transition()
	{
		if (defenceTimer.ExpiredOrNotRunning(owner.Runner))
		{
			ChangeToTrace();
			return;
		}

		if (owner.Agent.hasPath && owner.Agent.remainingDistance < 3f)
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

	private void ShieldBreak()
	{
		owner.SetAnimInt("Defence", 2);
		owner.AnimWaitStruct = new AnimWaitStruct("DefenceBreak", BruteZombie.State.Trace.ToString(), 1,
			updateAction: () =>
			{
				owner.SetAnimFloat("SpeedX", 0f, 0.5f);
				owner.SetAnimFloat("SpeedY", 0f, 0.5f);
			},
			exitAction: () => owner.SetAnimInt("Defence", 0));
		ChangeState(BruteZombie.State.AnimWait);
	}
}