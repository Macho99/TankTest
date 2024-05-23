using Fusion;
using System.Collections.Generic;
using UnityEngine;

public abstract class ZombieBaseIdle : ZombieBaseState
{
	float minIdle;
	float maxIdle;

	float idleShifter;
	TickTimer idleTimer;
	TickTimer stabilizeTimer;
	TickTimer soundTimer;

	public ZombieBaseIdle(ZombieBase owner, float minIdle, float maxIdle) : base(owner)
	{
		this.minIdle = minIdle;
		this.maxIdle = maxIdle;
	}

	public override void Enter()
	{
		idleShifter = Random.Range(0f, 2f);
		idleTimer = TickTimer.CreateFromSeconds(owner.Runner, Random.Range(minIdle, maxIdle));
		stabilizeTimer = TickTimer.CreateFromSeconds(owner.Runner, 2f);
		soundTimer = TickTimer.CreateFromSeconds(owner.Runner, Random.Range(0f, 5f));
	}

	public override void Exit()
	{
		owner.SetAnimFloat("SpeedX", 0f);
		owner.SetAnimFloat("SpeedY", 0f);
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		if (owner.Agent.hasPath &&  owner.Agent.desiredVelocity.sqrMagnitude > 0.1f)
		{
			OnTrace();
			return;
		}
	}

	protected abstract void OnTrace();

	protected abstract void OnIdleTimerExpired();

	public override void FixedUpdateNetwork()
	{
		if (soundTimer.Expired(owner.Runner))
		{
			owner.PlaySound(ZombieSoundType.Idle);
		}

		if (idleTimer.Expired(owner.Runner))
		{
			idleTimer = TickTimer.None;
			OnIdleTimerExpired();
		}

		if (stabilizeTimer.Expired(owner.Runner) == false)
		{
			owner.Decelerate(1f);
			owner.SetAnimFloat("IdleShifter", idleShifter, 0.2f);
		}
	}
}