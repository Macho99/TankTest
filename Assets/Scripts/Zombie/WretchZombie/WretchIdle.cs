using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class WretchIdle : ZombieBaseIdle
{
	new WretchZombie owner;

	public WretchIdle(WretchZombie owner) : base(owner, 2f, 10f)
	{
		this.owner = owner;
	}

	public override void SetUp()
	{

	}

	protected override void OnIdleTimerExpired()
	{
		float value = Random.value;
		if (value > 0.3)
		{
			owner.SetWanderDestination(30f);
		}
		else
		{
			PlayIdleAction();
		}
	}

	protected override void OnTrace()
	{
		ChangeState(WretchZombie.State.Trace);
	}

	private void PlayIdleAction()
	{
		owner.SetAnimFloat("ActionShifter", Random.Range(0, 15));
		owner.SetAnimTrigger("Action");
		owner.AnimWaitStruct = new AnimWaitStruct("Action", WretchZombie.State.Idle.ToString(), 
				updateAction: () =>
				{
					if (owner.Agent.hasPath && owner.Agent.desiredVelocity.sqrMagnitude > 0.1f)
					{
						ChangeState(WretchZombie.State.Trace);
					}
				}
			);
		ChangeState(WretchZombie.State.AnimWait);
	}
}