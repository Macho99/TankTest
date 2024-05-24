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

	public Int64 HitID => (owner.Object.Id.Raw << 32);

	protected ZombieBase owner;
	public ZombieBase Owner { get { return owner; } }

	protected virtual void Awake()
	{
		owner = GetComponentInParent<ZombieBase>();
	}

	public virtual void ApplyDamage(Transform source, Vector3 point, Vector3 force, int damage)
	{
		switch (bodyType)
		{
			case ZombieBody.Head:
				damage *= 2;
				break;
			case ZombieBody.MiddleSpine:
			case ZombieBody.Pelvis:
				//데미지 그대로
				break;
			default:
				damage = (int)(damage * 0.7f);
				break;
		}
		owner.ApplyDamage(source, this, point, force, damage);
	}
}