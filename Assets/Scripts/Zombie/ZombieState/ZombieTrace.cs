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
		prevPos = owner.transform.position;
		if (owner.Turned == false)
		{
			owner.TurnDirection = (owner.Target.position - owner.transform.position).normalized;
			ChangeState(Zombie.State.Turn);

			shifter = Random.Range(0, 5);
			speed = owner.TraceSpeed;
			rotateSpeed = 60f * speed;

			owner.AnimStartAction = SetShifterAndSpeedY;
			return;
		}
		else
		{
			StartCoroutine(CoSetDestination());
			owner.AnimStartAction = null;
		}
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
			owner.Follow = false;
			owner.Agent.ResetPath();
			ChangeState(Zombie.State.Idle);
		}

		owner.SetAnimFloat("SpeedX", speedX, 0.2f);
		owner.SetAnimFloat("SpeedY", speedY, 0.2f);
		owner.SetAnimFloat("Shifter", shifter, 0.2f);
	}

	private void SetShifterAndSpeedY()
	{
		owner.SetAnimFloat("Shifter", shifter);
		//owner.SetAnimFloat("SpeedY", speed);
	}

	private bool CheckFallAsleep()
	{
		Vector3 curPos = owner.transform.position;


		if(Mathf.Abs(curPos.y - prevPos.y) > owner.FallAsleepThreshold)
		{
			//owner.Agent.enabled = false;
			owner.transform.position = prevPos;
			ChangeState(Zombie.State.FallAsleep);
			return true;
		}
		prevPos = curPos;

		return false;
	}
}