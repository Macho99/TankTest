using System.Collections;
using UnityEngine;

public class ZombieStandUp : ZombieState
{
	public ZombieStandUp(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		owner.SetAnimBool("Crawl", false);
		owner.AnimNameToWaitEnd = "StandUp";
		owner.AfterAnimEndState = owner.StateDecision();
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
}