using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BruteStone : NetworkBehaviour
{
	[SerializeField] SphereCollider childCol;
	[SerializeField] int damage = 1000;

	Rigidbody rb;

	LayerMask hitMask;
	TickTimer despawnTimer;
	Collider[] cols = new Collider[10];
	List<Int64> hitList = new();

	[Networked] public NetworkId OwnerId { get; private set; }
	[Networked] public Vector3 Velocity { get; private set; }
	[Networked] public Vector3 AngularVelocity { get; private set; }

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		hitMask = LayerMask.GetMask("Vehicle", "Breakable", "Player");
	}

	public void Init(NetworkId id, Vector3 velocity)
	{
		this.OwnerId = id;

		rb.velocity = velocity;
		rb.angularVelocity = Vector3.up * Random.Range(5f, 10f);

		//this.Velocity = velocity;
		//AngularVelocity = Vector3.up * Random.Range(5f, 10f);
	}

	public override void Spawned()
	{
		despawnTimer = TickTimer.CreateFromSeconds(Runner, 500f);
	}

	public override void FixedUpdateNetwork()
	{
		CheckHit();

		if (despawnTimer.Expired(Runner))
		{
			Runner.Despawn(Object);
		}
	}

	private void CheckHit()
	{
		int result = Physics.OverlapSphereNonAlloc(childCol.transform.position, childCol.radius, cols, hitMask, QueryTriggerInteraction.Ignore);

		for (int i = 0; i < result; i++)
		{
			IHittable hittable = cols[i].GetComponentInParent<IHittable>();
			if(hittable == null) continue;
			if(hitList.Contains(hittable.HitID) == false)
			{
				hittable.ApplyDamage(null, childCol.transform.position, 
				//hittable.ApplyDamage(Runner.FindObject(OwnerId).transform, childCol.transform.position, 
				rb.velocity, (int) (damage * rb.velocity.magnitude));
				print($"{cols[i].gameObject.name}: {(int)(damage * rb.velocity.magnitude)} ¶§¸²");
				hitList.Add(hittable.HitID);
			}
		}
	}
}