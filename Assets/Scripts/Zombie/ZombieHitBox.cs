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

	private Zombie owner;
	public Zombie Owner { get
		{
			if(owner == null)
				owner = Root.GetComponent<Zombie>();
			return owner;
		}
	}

	public ZombieBody BodyType { get { return bodyType; } }

	public void ApplyDamage(Vector3 dir, float power, int damage)
	{
		Owner.ApplyDamage(bodyType, dir, power, damage);
	}
}