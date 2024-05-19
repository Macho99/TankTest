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

	float eatElapsed;
	bool eatEnd;

	TickTimer traceSoundTimer;
	TickTimer healTimer;
	Collider lastObstacleCol;
	float obstacleAttackTime;

	Collider[] cols = new Collider[1];

	public ZombieTrace(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		if (CheckTransition() == true) return;

		traceSoundTimer = TickTimer.CreateFromSeconds(owner.Runner, 5f);
		lastObstacleCol = null;
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
		if(owner.TargetData.IsTargeting == false)
		{
			if (owner.Agent.hasPath && owner.Agent.remainingDistance < 2f)
			{
				owner.Agent.ResetPath();
				ChangeState(Zombie.State.Idle);
				return true;
			}
		}
		else if(owner.TargetData.Layer == owner.MeatLayer)
		{
			if(owner.Agent.hasPath && owner.Agent.remainingDistance < 0.5f)
			{
				Eat();
				return true;
			}
		}
		else if (owner.AttackTargetMask.IsLayerInMask(owner.TargetData.Layer))
		{
			float speedY = owner.Anim.GetFloat("SpeedY");
			if(speedY > 1.8f && owner.TargetData.AbsAngle < 10f && owner.TargetData.Distance < 3f)
			{
				DashAttack();
			}
			else if(speedY > 2.8f && owner.TargetData.AbsAngle < 5f && owner.TargetData.Distance < 5f)
			{
				JumpAttack();
			}
			else if(owner.TargetData.Distance < 1.5f)
			{
				DirectionAttack();
				return true;
			}
			if (true == owner.TargetData.CheckObstacleAttack(owner.transform.position))
			{
				DirectionAttack();
				return true;
			}
		}
		return false;
	}

	private void Eat()
	{
		owner.PlaySound(ZombieSoundType.Eat);
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
						owner.TargetData.RemoveTarget();
						owner.SetAnimBool("Eat", false);
						return;
					}
				}

				if (owner.AttackTargetMask.IsLayerInMask(owner.TargetData.Layer))
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

	private void DashAttack()
	{
		owner.SetAnimFloat("TurnDir", 0f);
		owner.SetAnimFloat("AttackShifter", 3f);
		Attack();
	}

	private void JumpAttack()
	{
		owner.SetAnimFloat("TurnDir", 0f);
		owner.SetAnimFloat("AttackShifter", 4f);
		Attack();
	}

	private void DirectionAttack()
	{
		if(owner.TargetData.Sign < 0f && owner.TargetData.AbsAngle > 120f)
		{
			owner.SetAnimFloat("TurnDir", 2f);
		}
		else
		{
			owner.SetAnimFloat("TurnDir", owner.TargetData.Angle / 90f);
		}

		owner.SetAnimFloat("AttackShifter", Random.Range(0, 3));
		Attack();
	}

	private void Attack()
	{
		if (owner.Anim.GetFloat("IdleShifter") > 1.5f)
		{
			owner.Anim.SetFloat("IdleShifter", Random.Range(0f, 1f));
		}
		healTimer = TickTimer.CreateFromSeconds(owner.Runner, 0.4f);
		owner.SetAnimTrigger("Attack");
		owner.AnimWaitStruct = new AnimWaitStruct("Attack", "Trace",
			updateAction: () => {
				if (healTimer.Expired(owner.Runner))
				{
					healTimer = TickTimer.None;
					owner.Heal(10);
				}
				owner.Decelerate(1f);
			});
		owner.PlaySound(ZombieSoundType.Attack);
		ChangeState(Zombie.State.AnimWait);
	}

	public override void FixedUpdateNetwork()
	{
		if(CheckFallAsleep() == true)
		{
			return;
		}

		if(traceSoundTimer.Expired(owner.Runner))
		{
			owner.PlaySound(ZombieSoundType.Trace);
			traceSoundTimer = TickTimer.CreateFromSeconds(owner.Runner, 5f);
		}

		speed = owner.TraceSpeed;
		if(owner.TargetData.IsTargeting == false)
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
			cols, owner.DebrisMask);

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
				owner.PlaySound(ZombieSoundType.Hit);
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