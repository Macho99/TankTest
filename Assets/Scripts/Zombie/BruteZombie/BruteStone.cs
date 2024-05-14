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

	private Transform ownerTrans;
	[Networked, OnChangedRender(nameof(GetOwnerTransform))] public NetworkObject Owner { get; private set; }

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		hitMask = LayerMask.GetMask("Vehicle", "Breakable", "Player");
	}

	public void Init(NetworkObject owner, Vector3 velocity)
	{
		this.Owner = owner;

		rb.velocity = velocity;
		rb.angularVelocity = Vector3.up * Random.Range(5f, 10f);
		despawnTimer = TickTimer.CreateFromSeconds(Runner, 10f);
	}

	private void GetOwnerTransform()
	{
		ownerTrans = Owner.transform;
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
				hittable.ApplyDamage(ownerTrans, childCol.transform.position, 
				//hittable.ApplyDamage(Runner.FindObject(OwnerId).transform, childCol.transform.position, 
				rb.velocity, (int) (damage * rb.velocity.magnitude));
				print($"{cols[i].gameObject.name}: {(int)(damage * rb.velocity.magnitude)} ¶§¸²");
				hitList.Add(hittable.HitID);
			}
		}
	}
}