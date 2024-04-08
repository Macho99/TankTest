using System.Collections.Generic;
using UnityEngine;

public class ZombieIdle : ZombieState
{
	float idleTime;
	float elapsed;
	float shifter;

	public ZombieIdle(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		elapsed = 0f;
		idleTime = Random.Range(owner.MinIdleTime, owner.MaxIdleTime);
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

		if (owner.Target != null)
		{
			owner.CallChangeState(Zombie.State.Trace);
			return;
		}

		if(elapsed > idleTime)
		{
			owner.CallChangeState(Zombie.State.Wander);
			return;
		}
	}

	public override void Update()
	{
		elapsed += Time.deltaTime;
	}
}