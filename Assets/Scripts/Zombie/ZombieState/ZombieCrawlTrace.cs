﻿using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZombieCrawlTrace : ZombieState
{
	float speed;
	float rotateSpeed;

	bool attacked;
	float attackElapsed;

	bool eatEnd;
	float eatElapsed;

	TickTimer destinationTimer;

	public ZombieCrawlTrace(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		if (CheckTurn() == true) { return; }

		speed = 1f;
		rotateSpeed = 60f * speed;
		owner.SetAnimFloat("SpeedX", 0f);
	}

	public override void Exit()
	{

	}

	public override void FixedUpdateNetwork()
	{
		if (destinationTimer.ExpiredOrNotRunning(owner.Runner))
		{
			destinationTimer = TickTimer.CreateFromSeconds(owner.Runner, 0.5f);
			if (owner.Target != null)
				owner.Agent.SetDestination(owner.Target.position);
		}

		if (CheckTurn() == true) { return; }

		float speedY = 0f;
		if (owner.Agent.hasPath)
		{
			Vector3 lookDir;

			Vector3 moveDir = owner.Agent.desiredVelocity;
			if (moveDir.sqrMagnitude < 0.1f)
			{
				lookDir = (owner.Agent.steeringTarget - owner.transform.position).normalized;
				speedY = -1f;
			}
			else
			{
				moveDir.y = 0f;
				moveDir.Normalize();
				lookDir = moveDir;
				speedY = owner.transform.InverseTransformDirection(moveDir).z;
			}

			owner.transform.rotation = Quaternion.RotateTowards(owner.transform.rotation,
				Quaternion.LookRotation(lookDir), rotateSpeed * owner.Runner.DeltaTime);

			speedY *= speed;
		}
		owner.SetAnimFloat("SpeedY", speedY, 0.2f);
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		if (owner.CurLegHp > 0)
		{
			owner.SetAnimBool("Crawl", false);
			owner.AnimWaitStruct = new AnimWaitStruct("StandUp", owner.DecideState().ToString());
			ChangeState(Zombie.State.AnimWait);
			return;
		}

		if (owner.Agent.hasPath && owner.Agent.remainingDistance < 2f)
		{
			if (owner.Target == null)
			{
				owner.Agent.ResetPath();
				ChangeState(Zombie.State.CrawlIdle);
				return;
			}

			Vector3 headPos = owner.Head.position;
			headPos.y = owner.transform.position.y;
			Vector3 targetHeadVec = owner.Target.position - headPos;

			if (Vector3.Dot(targetHeadVec.normalized, owner.transform.forward) > Mathf.Cos(30f * Mathf.Deg2Rad))
			{
				Vector3 targetVec = owner.Target.position - owner.transform.position;
				float sqrMag = targetVec.sqrMagnitude;
				if(owner.CurTargetType == Zombie.TargetType.Meat)
				{
					if(owner.Agent.remainingDistance < 1f)
					{
						Eat();
					}
					return;
				}

				if (sqrMag < 3f)
				{
					if(sqrMag > 1.5f)
					{
						Attack(1);
					}
					else
					{
						Attack(0);
					}
				}
			}
		}
	}

	private void Attack(int attackShifter)
	{
		attacked = false;
		attackElapsed = 0f;
		owner.SetAnimFloat("AttackShifter", attackShifter);
		owner.SetAnimTrigger("Attack");
		owner.AnimWaitStruct = new AnimWaitStruct("CrawlAttack", "CrawlTrace",
			updateAction: () => {
				attackElapsed += owner.Runner.DeltaTime;
				if (attackElapsed > 0.18f && attacked == false)
				{
					attacked = true;
					owner.Attack(3 * (attackShifter + 1));
				}
				owner.SetAnimFloat("SpeedY", 0f, 1f);
			});
		ChangeState(Zombie.State.AnimWait);
	}

	private bool CheckTurn()
	{
		if (owner.Agent.hasPath == false) return false;
		Vector3 TurnDirection = owner.Agent.desiredVelocity.normalized;

		float angle = Vector3.SignedAngle(owner.transform.forward, TurnDirection, owner.transform.up);
		float sign = (angle >= 0f) ? 1f : -1f;
		angle = Mathf.Abs(angle);

		if (angle < 45f)
		{
			return false;
		}
		else if(angle > 90f)
		{
			owner.SetAnimFloat("TurnDir", sign);
		}
		else
		{
			owner.SetAnimFloat("TurnDir", sign * (angle) / 90f);
		}
		owner.SetAnimTrigger("Turn");
		owner.SetAnimFloat("SpeedY", 0f);
		owner.AnimWaitStruct = new AnimWaitStruct("CrawlTurn", Zombie.State.CrawlTrace.ToString());
		ChangeState(Zombie.State.AnimWait);
		return true;
	}

	private void Eat()
	{
		eatEnd = false;
		owner.Agent.ResetPath();
		owner.SetAnimBool("Eat", true);
		owner.AnimWaitStruct = new AnimWaitStruct("CrawlEat", Zombie.State.CrawlIdle.ToString(),
			updateAction: () =>
			{
				owner.SetAnimFloat("SpeedY", 0f, 0.2f);
				eatElapsed += owner.Runner.DeltaTime;
				if (eatElapsed > 2f && eatEnd == false)
				{
					eatElapsed = 0f;
					owner.Heal(20);
					if (owner.CurHp == owner.MaxHP)
					{
						eatEnd = true;
						owner.Target = null;
						owner.CurTargetType = Zombie.TargetType.None;
						owner.SetAnimBool("Eat", false);
						return;
					}
				}

				if (owner.CurTargetType == Zombie.TargetType.Player)
				{
					eatEnd = true;
					owner.SetAnimBool("Eat", false);
					return;
				}
			},
			animEndAction: () => owner.SetAnimBool("Eat", false)
			);
		ChangeState(Zombie.State.AnimWait);
	}
}