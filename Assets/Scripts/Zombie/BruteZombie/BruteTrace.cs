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

	}

	public override void Exit()
	{

	}

	public override void FixedUpdateNetwork()
	{
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

	public override void SetUp()
	{

	}

	public override void Transition()
	{

	}
}