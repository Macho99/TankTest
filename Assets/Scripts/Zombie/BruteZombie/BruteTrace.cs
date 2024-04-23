using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BruteTrace : BruteZombieState
{
	const float speed = 1f;
	const float rotateSpeed = 60f;

	public BruteTrace(BruteZombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		owner.OnHit += ChangeToDefence;
		if (CheckTransition() == true)
		{
			return;
		}
	}

	public override void Exit()
	{
		owner.OnHit -= ChangeToDefence;
	}

	private void ChangeToDefence()
	{
		owner.Shield.ResetHp();
		ChangeState(BruteZombie.State.DefenceTrace);
	}

	public override void FixedUpdateNetwork()
	{
		owner.LookTarget();
		owner.Trace(speed, rotateSpeed, 0.5f, 1f);
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		if(CheckTransition() == true)
		{
			return;
		}
	}

	private bool CheckTransition()
	{
		if (owner.Agent.hasPath && owner.Agent.remainingDistance < 1.5f)
		{
			if (owner.Target == null)
			{
				owner.Agent.ResetPath();
				ChangeState(BruteZombie.State.Idle);
				return true;
			}
			else if ((owner.Target.position - owner.transform.position).sqrMagnitude < 1.5f * 1.5f)
			{
				//Attack();
				return true;
			}
		}
		return false;
	}
}