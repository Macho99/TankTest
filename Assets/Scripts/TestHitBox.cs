using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestHitBox : ZombieHitBox
{
	public override void ApplyDamage(Transform source, Vector3 velocity, int damage)
	{
		GetComponent<Rigidbody>().AddForce(velocity * 5f, ForceMode.Impulse);
	}
}
