using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ZombieBaseDie : ZombieBaseState
{
	float despawnTime;
	protected TickTimer despawnTimer;

	public ZombieBaseDie(ZombieBase owner, float despawnTime = 10f) : base(owner)
	{
		this.despawnTime = despawnTime;
	}

	public override void Enter()
	{
		despawnTimer = TickTimer.CreateFromSeconds(owner.Runner, despawnTime);
	}

	public override void Exit()
	{

	}

	public override void FixedUpdateNetwork()
	{
		if(despawnTimer.Expired(owner.Runner))
		{
			despawnTimer = TickTimer.None;
			owner.Runner.Despawn(owner.Object);
		}
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{

	}
}