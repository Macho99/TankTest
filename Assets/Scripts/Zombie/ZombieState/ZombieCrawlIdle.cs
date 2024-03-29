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
		ChangeState(Zombie.State.StandUp);
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