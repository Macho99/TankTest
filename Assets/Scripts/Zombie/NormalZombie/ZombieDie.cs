using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ZombieDie : ZombieState
{
	const float despawnTime = 3f;
	float elapsed;

	public ZombieDie(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		elapsed = 0f;
	}

	public override void Exit()
	{

	}

	public override void FixedUpdateNetwork()
	{
		elapsed += owner.Runner.DeltaTime;
		
		if(elapsed > despawnTime)
		{
			owner.Runner.Despawn(owner.Object);
			ChangeState(Zombie.State.Wait);
		}
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{

	}
}