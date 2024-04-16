using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum ZombieBody { LeftHips, LeftKnee, RightHips, RightKnee,
	Head, Pelvis, MiddleSpine, LeftArm, LeftElbow, RightArm, RightElbow, 
	Size,
}

public class ZombieHitBox : Hitbox
{
	[SerializeField] ZombieBody bodyType;

	public ZombieBody BodyType { get { return bodyType; } }

	private Zombie owner;
	private Rigidbody rb;
	public Zombie Owner { get
		{
			if(owner == null)
				owner = Root.GetComponent<Zombie>();
			return owner;
		}
	}

	public Rigidbody Rigidbody { get
		{
			{
				if (rb == null)
					rb = GetComponent<Rigidbody>();
				return rb;
			}
		} }

	public void ApplyDamage(Transform source, Vector3 velocity, int damage)
	{
		Owner.ApplyDamage(source, this, velocity, damage);
	}
}