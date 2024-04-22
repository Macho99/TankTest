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

	int enterHitCnt;
	bool defence;

	public BruteTrace(BruteZombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		if (CheckTransition() == true)
		{
			return;
		}
		enterHitCnt = owner.HitCnt;
		defence = false;
	}

	public override void Exit()
	{

	}

	public override void FixedUpdateNetwork()
	{
		CheckDefence();

		owner.LookTarget();

		float speedX = 0f;
		float speedY = 0f;
		if (owner.Agent.hasPath)
		{
			Vector3 lookDir = (owner.Agent.steeringTarget - owner.transform.position);
			lookDir.y = 0f;
			lookDir.Normalize();
			owner.transform.rotation = Quaternion.RotateTowards(owner.transform.rotation,
				Quaternion.LookRotation(lookDir), rotateSpeed * owner.Runner.DeltaTime);
			Vector3 moveDir = owner.Agent.desiredVelocity.normalized;
			Vector3 animDir = owner.transform.InverseTransformDirection(moveDir);

			speedX = animDir.x * speed;
			speedY = animDir.z * speed;
		}

		owner.SetAnimFloat("SpeedX", speedX, 1f);
		owner.SetAnimFloat("SpeedY", speedY, 1f);
	}


	private void CheckDefence()
	{
		if(defence == false && enterHitCnt != owner.HitCnt)
		{
			defence = true;
			owner.SetAnimInt("Defence", 1);
		}
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