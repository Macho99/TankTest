using System.Collections.Generic;
using UnityEngine;

public class ZombieIdle : ZombieState
{
	float wanderThreshold = 0.3f;

	float idleTime;
	float elapsed;

	public ZombieIdle(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		elapsed = 0f;
		idleTime = Random.Range(4f, 10f);
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
		if (owner.Agent.desiredVelocity.sqrMagnitude > 0.1f)
		{
			ChangeState(Zombie.State.Trace);
			return;
		}

		if (elapsed > idleTime)
		{
			elapsed = -10f;
			float dice = Random.value;

			if(dice > wanderThreshold)
			{
				Vector3 pos = owner.transform.position;
				Vector3 randPos = pos + Random.insideUnitSphere * 20f;
				randPos.y = pos.y;
				owner.Agent.SetDestination(randPos);
				return;
			}
			else
			{
				Search();
				return;
			}
		}
	}

	public override void FixedUpdateNetwork()
	{
		elapsed += owner.Runner.DeltaTime;
		owner.SetAnimFloat("SpeedX", 0f);
		owner.SetAnimFloat("SpeedY", 0f, 1f);
	}

	private void Search()
	{
		owner.SetAnimTrigger("Search");
		owner.AnimWaitStruct = new AnimWaitStruct("Search", "Idle",
			updateAction: () =>
			{
				if (owner.Agent.desiredVelocity.sqrMagnitude > 0.1f)
				{
					owner.SetAnimTrigger("Exit");
					ChangeState(Zombie.State.Trace);
				}
			});
		ChangeState(Zombie.State.AnimWait);
	}
}