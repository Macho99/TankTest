using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZombieTrace : ZombieState
{
	float speed;
	float rotateSpeed;

	float prevPosY;
	float attackElapsed;
	bool attacked;

	float eatElapsed;
	bool eatEnd;

	Collider[] cols = new Collider[1];

	TickTimer destinationTimer;

	public ZombieTrace(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		if (CheckTransition() == true) return;

		prevPosY = owner.transform.position.y;
		//if (CheckTurn() == true) { return; }
	}


	public override void Exit()
	{

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
		if(owner.Target == null)
		{
			if (owner.Agent.hasPath && owner.Agent.remainingDistance < 2f)
			{
				owner.Agent.ResetPath();
				ChangeState(Zombie.State.Idle);
				return true;
			}
		}
		else if(owner.CurTargetType == Zombie.TargetType.Meat)
		{
			if(owner.Agent.hasPath && owner.Agent.remainingDistance < 0.5f)
			{
				Eat();
				return true;
			}
		}
		else if(owner.CurTargetType == ZombieBase.TargetType.Player)
		{
			if((owner.Target.position - owner.transform.position).sqrMagnitude < 1.5f * 1.5f)
			{
				Attack();
				return true;
			}
		}
		return false;
	}

	private void Eat()
	{
		eatEnd = false;
		owner.Agent.ResetPath();
		owner.SetAnimBool("Eat", true);
		owner.AnimWaitStruct = new AnimWaitStruct("EatEnd", Zombie.State.Idle.ToString(),
			updateAction: () =>
			{
				owner.SetAnimFloat("SpeedY", 0f, 0.2f);
				eatElapsed += owner.Runner.DeltaTime;
				if(eatElapsed > 2f && eatEnd == false)
				{
					eatElapsed = 0f;
					owner.Heal(20);
					if(owner.CurHp == owner.MaxHP)
					{
						eatEnd = true;
						owner.Target = null;
						owner.CurTargetType = Zombie.TargetType.None;
						owner.SetAnimBool("Eat", false);
						return;
					}
				}

				if(owner.CurTargetType == Zombie.TargetType.Player)
				{
					eatEnd = true;
					owner.SetAnimBool("Eat", false);
					return;
				}
			},
			exitAction: () => owner.SetAnimBool("Eat", false)
			);
		ChangeState(Zombie.State.AnimWait);
	}

	private void Attack()
	{
		if(owner.Anim.GetFloat("IdleShifter") > 1.5f)
		{
			owner.Anim.SetFloat("IdleShifter", Random.Range(0f, 1f));
		}

		Vector3 AttackDirection = (owner.Target.position - owner.transform.position).normalized;

		float angle = Vector3.SignedAngle(owner.transform.forward, AttackDirection, owner.transform.up);
		float sign = (angle >= 0f) ? 1f : -1f;

		angle = Mathf.Abs(angle);

		if(sign < 0f && angle > 120f)
		{
			owner.SetAnimFloat("TurnDir", 2f);
		}
		else
		{
			owner.SetAnimFloat("TurnDir", sign * (angle) / 90f);
		}

		attacked = false;
		attackElapsed = 0f;
		owner.SetAnimFloat("AttackShifter", Random.Range(0, 3));
		owner.SetAnimTrigger("Attack");
		owner.AnimWaitStruct = new AnimWaitStruct("Attack", "Trace", 
			updateAction: () => {
				attackElapsed += owner.Runner.DeltaTime;
				if(attackElapsed > 0.4f && attacked == false)
				{
					attacked = true;
					owner.Attack(10);
				}
				owner.Decelerate(1f);
			});
		ChangeState(Zombie.State.AnimWait);
	}

	public override void FixedUpdateNetwork()
	{
		if(CheckFallAsleep() == true)
		{
			return;
		}

		speed = owner.TraceSpeed;
		if(owner.Target == null)
		{
			speed = Mathf.Clamp(speed, 0f, 1f);
		}
		rotateSpeed = 60f * speed;

		owner.Trace(speed, rotateSpeed, 0.2f, 0.4f);
	}

	private bool CheckFallAsleep()
	{
		Vector3 curPos = owner.transform.position;

		float yDiff = Mathf.Abs(curPos.y - prevPosY);
		if (yDiff > owner.FallAsleepThreshold)
		{
			owner.SetAnimFloat("FallAsleep", 1f + yDiff - owner.FallAsleepThreshold);
			owner.SetAnimBool("Crawl", true);
			owner.AnimWaitStruct = new AnimWaitStruct("Fall", Zombie.State.CrawlIdle.ToString(),
				updateAction: ()=>owner.SetAnimFloat("SpeedY", 0f, 0.3f));
			ChangeState(Zombie.State.AnimWait);
			return true;
		}
		prevPosY = Mathf.Lerp(prevPosY, curPos.y, owner.Runner.DeltaTime * 5f);

		Collider prevCol, curCol;
		prevCol = cols[0];
		int cnt = Physics.OverlapSphereNonAlloc(curPos + Vector3.up * 0.3f + owner.transform.forward * 0.1f, 0.05f,
			cols, owner.FallAsleepMask);

		if (cnt > 0)
		{
			curCol = cols[0];
			if(curCol != prevCol)
			{
				float fallValue = curCol.bounds.size.y + 0.7f;
				owner.SetAnimFloat("FallAsleep", fallValue);
				owner.SetAnimBool("Crawl", true);
				owner.AnimWaitStruct = new AnimWaitStruct("Fall", Zombie.State.CrawlIdle.ToString(),
					updateAction: () => owner.SetAnimFloat("SpeedY", 0f, 0.3f));
				ChangeState(Zombie.State.AnimWait);
				return true;
			}
		}
		else
		{
			cols[0] = null;
		}

		return false;
	}

	private bool CheckTurn()
	{
		Vector3 TurnDirection = (owner.Agent.desiredVelocity).normalized;

		float angle = Vector3.SignedAngle(owner.transform.forward, TurnDirection, owner.transform.up);
		float sign = (angle >= 0f) ? 1f : -1f;
		angle = Mathf.Abs(angle);

        if (angle < 45f)
		{
			return false;
		}
		else
		{
			owner.SetAnimFloat("TurnDir", sign * (angle) / 90f);
		}
		owner.SetAnimTrigger("Turn");
		owner.AnimWaitStruct = new AnimWaitStruct("Turn", Zombie.State.Trace.ToString());
		ChangeState(Zombie.State.AnimWait);
		return true;
	}
}