using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum ZombieBody { LeftHips, LeftKnee, RightHips, RightKnee,
	Head, Pelvis, MiddleSpine, LeftArm, LeftElbow, RightArm, RightElbow, }

public class ZombieHitBox : Hitbox
{
	[SerializeField] ZombieBody bodyType;

	Zombie owner;

	public ZombieBody BodyType { get { return bodyType; } }

	private void Awake()
	{
		owner = Root.GetComponent<Zombie>();
	}

	public void ApplyDamage(Vector3 dir, float power, int damage)
	{
		owner.ApplyDamage(bodyType, dir, power, damage);
	}
}