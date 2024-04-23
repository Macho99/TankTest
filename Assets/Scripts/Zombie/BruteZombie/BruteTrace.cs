using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class BruteTrace : BruteZombieState
{
	const float speed = 1f;
	const float rotateSpeed = 60f;

	public BruteTrace(BruteZombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		if(owner.IsBerserk == false)
			owner.OnHit += ChangeToDefence;
		if (CheckTransition() == true)
		{
			return;
		}
	}

	public override void Exit()
	{
		if (owner.IsBerserk == false)
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
		if (owner.Target == null)
		{
			if (owner.Agent.hasPath && owner.Agent.remainingDistance < owner.AttackDist)
			{
				owner.Agent.ResetPath();
				ChangeState(BruteZombie.State.Idle);
				return true;
			}
			return false;
		}

		Vector3 attackPos = owner.transform.position;//TransformPoint(new Vector3(0f, 2.5f, 2f));
		print(attackPos);
		float dist = (attackPos - owner.Target.position).magnitude;
		print(dist);
		/*
		if (owner.IsBerserk)
		{
			//특수 공격 쿨타임 되면
			if (owner.SpecialAttackTimer.ExpiredOrNotRunning(owner.Runner))
			{

			}
		}*/
		if (dist < owner.AttackDist)
		{
			Attack((BruteZombie.AttackType) Random.Range(0, 10));
			return true;
		}
		return false;
	}

	private void Attack(BruteZombie.AttackType type)
	{
		owner.SetAnimFloat("ActionShifter", (int)type);
		owner.SetAnimTrigger("Attack");
		owner.AnimWaitStruct = new AnimWaitStruct("Attack", BruteZombie.State.Trace.ToString(), 
			startAction: () => owner.LookWeight = 0f,
			updateAction: owner.Decelerate, 
			exitAction: () => owner.LookWeight = 1f);
		ChangeState(BruteZombie.State.AnimWait);
	}
}