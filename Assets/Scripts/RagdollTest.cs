using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollTest : NetworkBehaviour
{
	Rigidbody rb;

	private void Awake()
	{
		rb = GetComponentInChildren<Rigidbody>();
	}

	public override void Spawned()
	{
		rb.AddForce(Vector3.up * 1000f, ForceMode.Impulse);
	}
}
