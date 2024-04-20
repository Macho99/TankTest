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

public class ZombieHitBox : MonoBehaviour
{
	[SerializeField] ZombieBody bodyType;
	[SerializeField] Rigidbody rb;

	public ZombieBody BodyType { get { return bodyType; } }
	public Rigidbody RB { get { return rb; } }

	private Zombie owner;

	private void Awake()
	{
		owner = GetComponentInParent<Zombie>();
	}

	public virtual void ApplyDamage(Transform source, Vector3 velocity, int damage)
	{
		owner.ApplyDamage(source, this, velocity, damage);
	}
}