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

	float prevPosY;
	Collider[] cols = new Collider[1];

	public ZombieTrace(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		prevPosY = owner.transform.position.y;
		if (CheckTurn() == true) { return; }

		speed = owner.TraceSpeed;
		rotateSpeed = 60f * speed;
		StartCoroutine(CoSetDestination());
	}

	private IEnumerator CoSetDestination()
	{
		while (true)
		{
			owner.SetDestination(owner.Target.transform.position);
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

	public override void FixedUpdateNetwork()
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
				Quaternion.LookRotation(lookDir), rotateSpeed * owner.Runner.DeltaTime);
			Vector3 moveDir = owner.DesiredDir;
			Vector3 animDir = owner.transform.InverseTransformDirection(moveDir);

			speedX = animDir.x * speed;
			speedY = animDir.z * speed;
		}

		if (owner.HasPath && owner.RemainDist < 1f)
		{
			owner.Runner.Despawn(owner.Object);
			ChangeState(Zombie.State.Wait);
			return;
			//shifter = Random.Range(0, 5);
			//owner.Agent.ResetPath();
			//ChangeState(Zombie.State.Idle);
		}

		owner.SetAnimFloat("SpeedX", speedX, 0.2f);
		owner.SetAnimFloat("SpeedY", speedY, 0.2f);
		owner.SetAnimFloat("Shifter", shifter, 0.2f);
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
		Vector3 TurnDirection = (owner.Target.transform.position - owner.transform.position).normalized;

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