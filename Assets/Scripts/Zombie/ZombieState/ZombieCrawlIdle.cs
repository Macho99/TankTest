using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ZombieCrawlIdle : ZombieState
{
	public ZombieCrawlIdle(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		StandUp();
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

	private void StandUp()
	{
		owner.SetAnimBool("Crawl", false);
		owner.AnimWaitStruct = new AnimWaitStruct("StandUp", owner.DecisionState().ToString());
		ChangeState(Zombie.State.AnimWait);
	}
}