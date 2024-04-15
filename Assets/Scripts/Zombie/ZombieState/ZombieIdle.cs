using System.Collections.Generic;
using UnityEngine;

public class ZombieIdle : ZombieState
{
	float idleTime;
	float elapsed;

	public ZombieIdle(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		elapsed = 0f;
		idleTime = Random.Range(owner.MinIdleTime, owner.MaxIdleTime);
	}

	public override void Exit()
	{

	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		if (owner.Target != null)
		{
			ChangeState(Zombie.State.Trace);
			return;
		}

		if(elapsed > idleTime)
		{
			ChangeState(Zombie.State.Wander);
			return;
		}
	}

	public override void FixedUpdateNetwork()
	{
		elapsed += owner.Runner.DeltaTime;
	}
}