using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ZombieCrawlTrace : ZombieState
{
	float speed;
	float rotateSpeed;

	public ZombieCrawlTrace(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		if (CheckTurn() == true) { return; }

		speed = 1f;
		rotateSpeed = 60f * speed;
		owner.SetAnimFloat("SpeedX", 0f);
		StartCoroutine(CoSetDestination());
	}
	private IEnumerator CoSetDestination()
	{
		while (true)
		{
			owner.Agent.SetDestination(owner.Target.position);
			yield return new WaitForSeconds(0.5f);
		}
	}

	public override void Exit()
	{

	}

	public override void FixedUpdateNetwork()
	{
		if(CheckTurn() == true) { return; }

		float speedY = 0f;
		if (owner.Agent.hasPath)
		{
			Vector3 moveDir = owner.Agent.desiredVelocity;
			moveDir.y = 0f;
			moveDir.Normalize();
			owner.transform.rotation = Quaternion.RotateTowards(owner.transform.rotation,
				Quaternion.LookRotation(moveDir), rotateSpeed * owner.Runner.DeltaTime);
			Vector3 animDir = owner.transform.InverseTransformDirection(moveDir);

			speedY = animDir.z * speed;
		}

		if (owner.Agent.hasPath && owner.Agent.remainingDistance < 1f)
		{
			owner.Runner.Despawn(owner.Object);
			ChangeState(Zombie.State.Wait);
			return;
		}

		owner.SetAnimFloat("SpeedY", speedY, 0.2f);
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{

	}

	private bool CheckTurn()
	{
		Vector3 TurnDirection = (owner.Target.transform.position - owner.transform.position).normalized;

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
}