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
	float shifter;
	float speed;
	float rotateSpeed;

	Vector3 prevPos;

	public ZombieTrace(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		if (CheckTurn() == true) { return; }

		speed = owner.TraceSpeed;
		rotateSpeed = 60f * speed;
		StartCoroutine(CoSetDestination());
	}

	private IEnumerator CoSetDestination()
	{
		while (true)
		{
			owner.SetDestination(owner.Target.position);
			yield return new WaitForSeconds(0.5f);
		}
	}

	public override void Exit()
	{

	}

	public override void SetUp()
	{
		shifter = Random.Range(0, 5);
	}

	public override void Transition()
	{

	}

	public override void Update()
	{
		if(CheckFallAsleep() == true)
		{
			return;
		}

		float speedX = 0f;
		float speedY = 0f;
		if (owner.HasPath)
		{
			Vector3 lookDir = (owner.SteeringTarget - owner.transform.position);
			lookDir.y = 0f;
			lookDir.Normalize();
			owner.transform.rotation = Quaternion.RotateTowards(owner.transform.rotation,
				Quaternion.LookRotation(lookDir), rotateSpeed * Time.deltaTime);
			Vector3 moveDir = owner.DesiredDir;
			Vector3 animDir = owner.transform.InverseTransformDirection(moveDir);

			speedX = animDir.x * speed;
			speedY = animDir.z * speed;
		}
		else if(owner.Agent.pathPending == false && owner.RemainDist < 1f)
		{
			shifter = Random.Range(0, 5);
			owner.Agent.ResetPath();
			ChangeState(Zombie.State.Idle);
		}

		owner.SetAnimFloat("SpeedX", speedX, 0.2f);
		owner.SetAnimFloat("SpeedY", speedY, 0.2f);
		owner.SetAnimFloat("Shifter", shifter, 0.2f);
	}

	private bool CheckFallAsleep()
	{
		Vector3 curPos = owner.transform.position;

		if(Mathf.Abs(curPos.y - prevPos.y) > owner.FallAsleepThreshold)
		{
			owner.SetAnimFloat("FallAsleep", 1f);
			owner.SetAnimBool("Crawl", true);
			owner.AnimWaitStruct = new AnimWaitStruct("Fall", Zombie.State.CrawlIdle.ToString(),
				updateAction: ()=>owner.SetAnimFloat("SpeedY", 0f, 0.3f));
			ChangeState(Zombie.State.AnimWait);
			return true;
		}
		prevPos = curPos;

		if(Physics.CheckSphere(curPos + Vector3.up * 0.3f + owner.transform.forward * 0.1f, 0.05f, 
			LayerMask.NameToLayer("FallAsleepObject")))
		{
			print(LayerMask.NameToLayer("FallAsleepObject"));
			owner.SetAnimFloat("FallAsleep", 1f);
			owner.SetAnimBool("Crawl", true);
			owner.AnimWaitStruct = new AnimWaitStruct("Fall", Zombie.State.CrawlIdle.ToString(),
				updateAction: () => owner.SetAnimFloat("SpeedY", 0f, 0.3f));
			ChangeState(Zombie.State.AnimWait);
			return true;
		}

		return false;
	}

	private bool CheckTurn()
	{
		prevPos = owner.transform.position;
		Vector3 TurnDirection = (owner.Target.position - owner.transform.position).normalized;

		float angle = Vector3.SignedAngle(owner.transform.forward, TurnDirection, owner.transform.up);
		float sign = (angle >= 0f) ? 1f : -1f;
		angle = Mathf.Abs(angle);

		if (angle < 60f)
		{
			return false;
		}
		else if (angle < 135f)
		{
			owner.SetAnimFloat("TurnDir", sign);
		}
		else
		{
			owner.SetAnimFloat("TurnDir", 0f);
		}
		owner.SetAnimTrigger("Turn");
		owner.AnimWaitStruct = new AnimWaitStruct("Turn", Zombie.State.Trace.ToString(), 
			animStartAction: () => owner.SetAnimFloat("Shifter", shifter));
		ChangeState(Zombie.State.AnimWait);
		return true;
	}
}