using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ZombieFallAsleep : ZombieState
{
	bool animPlayed;

	public ZombieFallAsleep(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		animPlayed = false;
		owner.SetAnimFloat("FallAsleep", 1f);
		owner.SetAnimBool("Crawl", true);
	}

	public override void Exit()
	{
		owner.Agent.enabled = true;
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		if(animPlayed == true)
		{
			if(owner.IsAnimName("Fall") == false)
			{
				ChangeState(Zombie.State.CrawlIdle);
			}
		}
	}

	public override void Update()
	{
		if(animPlayed == true) { return; }

		if(owner.IsAnimName("Fall") == true)
		{
			animPlayed = true;
		}
	}
}