using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class BruteIdle : BruteZombieState
{
	float idleTime;
	float elapsed;

	const float wanderTreshold = 0.5f;

	public BruteIdle(BruteZombie owner) : base(owner)
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
			ChangeState(BruteZombie.State.Trace);
			return;
		}

		if (elapsed > idleTime)
		{
			elapsed = -10f;
			float dice = Random.value;

			if (dice > wanderTreshold)
			{
				Vector3 pos = owner.transform.position;
				Vector3 randPos = pos + Random.insideUnitSphere * 20f;
				randPos.y = pos.y;
				owner.Agent.SetDestination(randPos);
				return;
			}
			else
			{
				//ChangeState(BruteZombie.State.Search);
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
}