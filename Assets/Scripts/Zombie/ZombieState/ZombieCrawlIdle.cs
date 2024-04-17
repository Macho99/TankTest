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
		if(owner.Target != null)
		{
			ChangeState(Zombie.State.CrawlTrace);
		}
	}

	public override void FixedUpdateNetwork()
	{

	}

	private void StandUp()
	{
		if(owner.CurLegHp > 0)
		{
			owner.SetAnimBool("Crawl", false);
			owner.AnimWaitStruct = new AnimWaitStruct("StandUp", owner.DecideState().ToString());
			ChangeState(Zombie.State.AnimWait);
		}
	}
}