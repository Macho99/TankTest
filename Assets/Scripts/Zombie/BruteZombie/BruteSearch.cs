using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

public class BruteSearch : BruteZombieState
{
	int maxLook;
	int curLook;
	TickTimer lookTimer;
	float minLookTime = 3f;
	float maxLookTime = 5f;
	enum Direction { Left, Right };
	Direction curDir;

	public BruteSearch(BruteZombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		curLook = 0;
		maxLook = Random.Range(5, 8);
		curDir = (Direction)Random.Range(0, 2);
	}

	public override void Exit()
	{

	}

	public override void FixedUpdateNetwork()
	{
		if (lookTimer.ExpiredOrNotRunning(owner.Runner))
		{
			RefreshLook();
		}
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
	}

	private void RefreshLook()
	{
		curLook++;
		if(curLook > maxLook)
		{
			ChangeState(BruteZombie.State.Idle);
			return;
		}
		else if(curLook % 2 == 0)
		{
			lookTimer = TickTimer.CreateFromSeconds(owner.Runner, 0.8f);
			owner.LookTarget();
		}
		else
		{
			lookTimer = TickTimer.CreateFromSeconds(owner.Runner, Random.Range(minLookTime, maxLookTime));
			curDir = (curDir == Direction.Left) ? Direction.Right : Direction.Left;
			float dir = curDir == Direction.Right ? 1f : -1f;
			owner.LookPos = owner.transform.TransformPoint(
				(Vector3.forward + (Vector3.right * dir) * Random.Range(1f, 3f)).normalized * 5f);
		}

	}
}