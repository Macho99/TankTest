using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZombieCrawlIdle : ZombieState
{
	float elapsed;
	float idleTime;

	public ZombieCrawlIdle(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		elapsed = 0f;
		idleTime = Random.Range(3f, 30f);
		CheckStandUp();
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
			return;
		}

		if (elapsed > idleTime)
		{
			owner.SetAnimTrigger("Search");
			owner.AnimWaitStruct = new AnimWaitStruct("CrawlSearch", "CrawlIdle",
				updateAction: () =>
				{
					if (owner.Target != null)
					{
						owner.SetAnimTrigger("Exit");
						ChangeState(Zombie.State.CrawlTrace);
					}
				});
			ChangeState(Zombie.State.AnimWait);
			return;
		}
	}

	public override void FixedUpdateNetwork()
	{
		elapsed += owner.Runner.DeltaTime;
		owner.SetAnimFloat("SpeedY", 0f, 1f);
	}

	private void CheckStandUp()
	{
		if(owner.CurLegHp > 0)
		{
			owner.SetAnimBool("Crawl", false);
			owner.AnimWaitStruct = new AnimWaitStruct("StandUp", owner.DecideState().ToString());
			ChangeState(Zombie.State.AnimWait);
		}
	}
}