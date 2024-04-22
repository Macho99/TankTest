using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ShieldHitBox : ZombieHitBox
{
	[SerializeField] int maxShieldHp = 1000;

	private BruteZombie bruteOwner;
	public int CurShieldHp { get; set; }

	protected override void Awake()
	{
		base.Awake();
		bruteOwner = owner as BruteZombie;
		bodyType = ZombieBody.Shield;
		rb = GetComponent<Rigidbody>();
	}

	public void ResetHp()
	{
		CurShieldHp = maxShieldHp;
	}

	public override void ApplyDamage(Transform source, Vector3 velocity, int damage)
	{
		if(CurShieldHp > 0)
		{
			CurShieldHp -= damage;
			damage /= 5;
		}
		else
		{
			bruteOwner.ShieldBreak();
		}
		base.ApplyDamage(source, velocity, damage);
	}
}
