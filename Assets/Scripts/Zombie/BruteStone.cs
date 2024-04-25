using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BruteStone : NetworkBehaviour
{
	Rigidbody rb;
	Collider col;

	TickTimer despawnTimer;
	[Networked] public NetworkId OwnerId { get; private set; }
	[Networked] public Vector3 Velocity { get; private set; }
	[Networked] public Vector3 AngularVelocity { get; private set; }

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		col = GetComponent<Collider>();
	}

	public void Init(Vector3 velocity)
	{
		this.Velocity = velocity;
		AngularVelocity = Vector3.up * Random.Range(5f, 10f);
		despawnTimer = TickTimer.CreateFromSeconds(Runner, 5f);
	}

	public override void Spawned()
	{
		rb.velocity = Velocity;
		rb.angularVelocity = AngularVelocity;
	}

	public override void FixedUpdateNetwork()
	{
		if (despawnTimer.Expired(Runner))
		{
			Runner.Despawn(Object);
		}
	}
}