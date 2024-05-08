using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class BruteIdle : ZombieBaseIdle
{
	new BruteZombie owner;
	const float wanderTreshold = 0.5f;

	public BruteIdle(BruteZombie owner) : base(owner, 4f, 10f)
	{
		this.owner = owner;
	}

	public override void Enter()
	{
		base.Enter();
		owner.LookTarget();
	}

	protected override void OnIdleTimerExpired()
	{
		float dice = Random.value;

		if (dice > wanderTreshold)
		{
			Vector3 pos = owner.transform.position;
			Vector3 randPos = pos + Random.insideUnitSphere * 20f;
			randPos.y = pos.y;
			owner.Agent.SetDestination(randPos);
			return;
		}
		else
		{
			ChangeState(BruteZombie.State.Search);
			return;
		}
	}

	protected override void OnTrace()
	{
		ChangeState(BruteZombie.State.Trace);
	}
}