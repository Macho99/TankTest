using System.Collections.Generic;
using UnityEngine;

public class ZombieIdle : ZombieState
{
	float idleTime;
	float elapsed;
	float shifter;

	public ZombieIdle(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		elapsed = 0f;
		shifter = Random.Range(0, 3);
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
		if(owner.Target != null)
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

	public override void Update()
	{
		owner.SetAnimFloat("Shifter", shifter, 0.1f);
		elapsed += owner.Runner.DeltaTime;
	}
}