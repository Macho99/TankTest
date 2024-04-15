using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ZombieRagdollEnter : ZombieState
{
	float elapsed;
	float exitTime;

	public ZombieRagdollEnter(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		elapsed = 0f;
		exitTime = 2f;
	}

	public override void Exit()
	{
	}

	public override void FixedUpdateNetwork()
	{
		elapsed += owner.Runner.DeltaTime;
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		if(elapsed > exitTime)
		{
			owner.IsRagdoll = false;
		}
	}
}