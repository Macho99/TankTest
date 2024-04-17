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
		idleTime = Random.Range(4f, 30f);
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
		if (owner.Target != null)
		{
			ChangeState(Zombie.State.Trace);
			return;
		}

		if (elapsed > idleTime)
		{
			owner.SetAnimTrigger("Search");
			owner.AnimWaitStruct = new AnimWaitStruct("Search", "Idle",
				updateAction: () =>
				{
					if(owner.Target != null)
					{
						owner.SetAnimTrigger("Exit");
						ChangeState(Zombie.State.Trace);
					}
				});
			ChangeState(Zombie.State.AnimWait);
			//ChangeState(Zombie.State.Wander);
			return;
		}
	}

	public override void FixedUpdateNetwork()
	{
		elapsed += owner.Runner.DeltaTime;
		owner.SetAnimFloat("SpeedX", 0f);
		owner.SetAnimFloat("SpeedY", 0f, 1f);
	}
}