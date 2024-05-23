using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZombieCrawlIdle : ZombieBaseIdle
{
	new Zombie owner;
	public ZombieCrawlIdle(Zombie owner) : base(owner, 3f, 20f)
	{
		this.owner = owner;
	}

	public override void Enter()
	{
		base.Enter();
		CheckStandUp();
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

	protected override void OnTrace()
	{
		ChangeState(Zombie.State.CrawlTrace);
	}

	protected override void OnIdleTimerExpired()
	{
		owner.SetAnimTrigger("Search");
		owner.AnimWaitStruct = new AnimWaitStruct("CrawlSearch", "CrawlIdle",
			updateAction: () =>
			{
				if (owner.Agent.desiredVelocity.sqrMagnitude > 0.1f)
				{
					owner.SetAnimTrigger("Exit");
					ChangeState(Zombie.State.CrawlTrace);
				}
			});
		ChangeState(Zombie.State.AnimWait);
	}
}