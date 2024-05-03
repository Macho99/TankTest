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

public class ZombieHitBox : MonoBehaviour, IHittable
{
	[SerializeField] protected ZombieBody bodyType;
	[SerializeField] protected Rigidbody rb;

	public ZombieBody BodyType { get { return bodyType; } }
	public Rigidbody RB { get { return rb; } }

	public uint HitID => owner.Object.Id.Raw;

	protected ZombieBase owner;

	protected virtual void Awake()
	{
		owner = GetComponentInParent<ZombieBase>();
	}

	public virtual void ApplyDamage(Transform source, Vector3 point, Vector3 force, int damage)
	{
		owner.ApplyDamage(source, this, point, force, damage);
	}
}