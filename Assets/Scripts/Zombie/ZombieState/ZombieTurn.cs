using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ZombieTurn : ZombieState
{
	bool complete;

	public ZombieTurn(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		complete = false;
		owner.Turned = true;

		float angle = Vector3.SignedAngle(owner.transform.forward, owner.TurnDirection, owner.transform.up);
		float sign = (angle >= 0f) ? 1f : -1f;
		angle = Mathf.Abs(angle);

		if(angle < 45f)
		{
			ChangeState(GetNextState());
			return;
		}
		else if(angle < 135f)
		{
			owner.SetAnimTrigger("Turn");
			owner.SetAnimFloat("TurnDir", sign);
		}
		else
		{
			owner.SetAnimTrigger("Turn");
			owner.SetAnimFloat("TurnDir", 0f);
		}
		owner.AnimNameToWaitEnd = "Turn";
		owner.AfterAnimEndState = GetNextState();
		ChangeState(Zombie.State.AnimWait);
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

	}

	private Zombie.State GetNextState()
	{
		if(owner.Target != null && owner.Follow == true)
		{
			return Zombie.State.Trace;
		}
		else
		{
			return Zombie.State.Wander;
		}
	}
}