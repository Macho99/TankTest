using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ZombieSteer : ZombieState
{
	public ZombieSteer(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		Vector3 steerDir = (owner.SteeringTarget - owner.transform.position).normalized;
		float dotResult = Vector3.Dot(owner.transform.forward, steerDir);
		// 다음 경로 방향과 현재 forward 방향 비교

		// 
		if (dotResult > Mathf.Cos(45 * Mathf.Deg2Rad))
		{
			ChangeState(Zombie.State.Trace);
			return;
		}
		//else if(dotResult < Mathf.Cos())
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
}