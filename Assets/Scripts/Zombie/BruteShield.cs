using ExitGames.Client.Photon.StructWrapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BruteShield : MonoBehaviour, IHittable
{
	[SerializeField] int maxShieldHp = 1000;

	Rigidbody rb;
	Collider col;
	BruteDefenceTrace owner;

	public bool Enabled { get; private set; }
	public int CurShieldHp { get; set; }

	private void Awake()
	{
		col = GetComponent<Collider>();
		rb = GetComponent<Rigidbody>();
	}

	public void ResetHp()
	{
		CurShieldHp = maxShieldHp;
	}

	public void SetEnable(bool value)
	{
		Enabled = value;
		col.enabled = value;
	}

	public void Init(BruteDefenceTrace owner)
	{
		this.owner = owner;
	}

	public void Break()
	{
		transform.parent = null;
		col.isTrigger = false;
		rb.useGravity = true;
		rb.isKinematic = false;
	}

	public void ApplyDamage(Transform source, Vector3 velocity, int damage)
	{
		if(owner == null)
		{
			Debug.LogError("Enable false 상태인데 ApplyDamage 시도");
			return;
		}

		CurShieldHp -= damage;
		print(CurShieldHp);
		if (CurShieldHp > 0)
		{
			owner.ResetTimer();
			if(damage > 100)
			{
				owner.Knockback();
			}
		}
		else
		{
			owner.ShieldBreak();
			print("쉴드 부사짐");
		}
	}
}