using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class BruteRoar : BruteZombieState
{
	bool animEntered;
	TickTimer spawnTimer;
	ZombieSpawner spawner;
	Transform target;

	public ZombieSpawner Spawner { get 
		{ 
			if (spawner == null)
			{
				spawner = owner.Runner.GetComponent<ZombieSpawner>();
			}
			return spawner;
		}
	}

	public BruteRoar(BruteZombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		target = owner.TargetData.Transform;
		owner.LookWeight = 0f;
		animEntered = false;
		spawnTimer = TickTimer.CreateFromSeconds(owner.Runner, 2f);
		owner.SetAnimTrigger("Roar");
	}

	public override void Exit()
	{
		owner.LookWeight = 1f;
	}

	public override void FixedUpdateNetwork()
	{
		owner.Decelerate();

		if (spawnTimer.ExpiredOrNotRunning(owner.Runner))
		{
			spawnTimer = TickTimer.CreateFromSeconds(owner.Runner, Random.Range(0.2f, 0.5f));
			Spawner.SpawnZombie(BeforeSpawned);
		}

		if (animEntered == false && owner.IsAnimName("Roar") == true)
		{
			animEntered = true;
		}
	}

	private void BeforeSpawned(NetworkRunner runner, NetworkObject netObj)
	{
		Random.InitState(runner.SessionInfo.Name.GetHashCode() * netObj.Id.Raw.GetHashCode());

		Vector3 pos = Random.insideUnitSphere * 10f;
		pos.y = 0f;
		Zombie zombie = netObj.GetComponent<Zombie>();
		zombie.transform.rotation = Quaternion.LookRotation(new Vector3(Random.value, 0f, Random.value));
		zombie.transform.position = owner.transform.position + pos;
		zombie.TargetData.SetTarget(target);
	}

	public override void SetUp()
	{
	}

	public override void Transition()
	{
		if(animEntered == true && owner.IsAnimName("Roar") == false)
		{
			ChangeState(BruteZombie.State.Idle);
		}
	}
}