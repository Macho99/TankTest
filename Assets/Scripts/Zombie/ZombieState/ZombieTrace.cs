using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ZombieTrace : ZombieState
{
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
		if (photonView.IsMine)
		{
			if (CheckTurn() == true) { return; }
			StartCoroutine(CoSetDestination());
		}

		speed = owner.MaxTraceSpeed;
		rotateSpeed = 60f * speed;
	}

	private IEnumerator CoSetDestination()
	{
		while (true)
		{
			owner.photonView.RPC(nameof(owner.SetTracePath), 
				RpcTarget.AllViaServer, owner.transform.position, owner.Target.transform.position);
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
		if (photonView.IsMine == false) return;

		if (CheckFallAsleep() == true)
		{
			return;
		}

		if (owner.HasPath && owner.Agent.remainingDistance < 1f)
		{
			PhotonNetwork.Destroy(photonView);
			ChangeState(Zombie.State.Wait);
		}
	}

	public override void Update()
	{
		if (photonView.IsMine == false) return;

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


		owner.SetAnimFloat("SpeedX", speedX, 0.2f);
		owner.SetAnimFloat("SpeedY", speedY, 0.2f);
	}

	private bool CheckFallAsleep()
	{
		Vector3 curPos = owner.transform.position;

		float yDiff = Mathf.Abs(curPos.y - prevPosY);
		if (yDiff > owner.FallAsleepThreshold)
		{
			ChangeState(Zombie.State.Wait);
			photonView.RPC(nameof(owner.FallAsleepRPC), RpcTarget.AllViaServer, 1f + yDiff - owner.FallAsleepThreshold);
			return true;
		}
		prevPosY = Mathf.Lerp(prevPosY, curPos.y, Time.deltaTime * 5f);

		Collider prevCol, curCol;
		prevCol = cols[0];
		int cnt = Physics.OverlapSphereNonAlloc(curPos + Vector3.up * 0.3f + owner.transform.forward * 0.1f, 0.05f,
			cols, owner.FallAsleepMask);

		if (cnt > 0)
		{
			curCol = cols[0];
			if(curCol != prevCol)
			{
				ChangeState(Zombie.State.Wait);
				photonView.RPC(nameof(owner.FallAsleepRPC), RpcTarget.AllViaServer, curCol.bounds.size.y + 0.7f);
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

		float turnDir;
		if (angle < 60f)
		{
			return false;
		}
		else if (angle < 135f)
		{
			turnDir = sign;
		}
		else
		{
			turnDir = 0f;
		}

		ChangeState(Zombie.State.Wait);
		photonView.RPC(nameof(owner.TurnRPC), RpcTarget.AllViaServer, turnDir);
		return true;
	}
}