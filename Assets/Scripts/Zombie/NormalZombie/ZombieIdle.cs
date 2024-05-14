using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZombieIdle : ZombieBaseIdle
{
	float wanderThreshold = 0.3f;

	new Zombie owner;

	public ZombieIdle(Zombie owner) : base(owner, 4f, 10f)
	{
		this.owner = owner;
	}

	protected override void OnIdleTimerExpired()
	{
		float dice = Random.value;

		if (dice > wanderThreshold)
		{
			owner.SetWanderDestination(20f);
			return;
		}
		else
		{
			Search();
			return;
		}
	}

	protected override void OnTrace()
	{
		ChangeState(Zombie.State.Trace);
	}

	private void Search()
	{
		owner.SetAnimTrigger("Search");
		owner.AnimWaitStruct = new AnimWaitStruct("Search", "Idle",
			updateAction: () =>
			{
				if (owner.Agent.desiredVelocity.sqrMagnitude > 0.1f)
				{
					owner.SetAnimTrigger("Exit");
					ChangeState(Zombie.State.Trace);
				}
			});
		ChangeState(Zombie.State.AnimWait);
	}
}