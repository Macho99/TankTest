using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ShieldHitBox : ZombieHitBox
{
	public int ShieldHp { get; set; }

	protected override void Awake()
	{
		base.Awake();
		bodyType = ZombieBody.Shield;
		rb = GetComponent<Rigidbody>();
	}

	public override void ApplyDamage(Transform source, Vector3 velocity, int damage)
	{
		base.ApplyDamage(source, velocity, damage);
	}
}
