using ExitGames.Client.Photon.StructWrapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class BruteShield : MonoBehaviour, IHittable
{
	[SerializeField] int maxShieldHp = 1000;

	Rigidbody rb;
	Collider col;
	BruteDefenceTrace owner;
	Vector3 lastVel;
	Vector3 lastPoint;

	public bool Enabled { get; private set; }
	public int CurShieldHp { get; set; }

	public uint HitID => owner.Owner.Object.Id.Raw;

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
		Enabled = false;
		transform.parent = null;
		col.isTrigger = false;
		col.enabled = true;
		rb.useGravity = true;
		rb.isKinematic = false;
		rb.AddForceAtPosition(lastVel, lastPoint, ForceMode.Impulse);
		Destroy(gameObject, 5f);
	}

	public void ApplyDamage(Transform source, Vector3 point, Vector3 velocity, int damage)
	{
		if(Enabled == false) { return; }
		if(owner == null)
		{
			Debug.LogError("Enable false 상태인데 ApplyDamage 시도");
			return;
		}

		lastPoint = point;
		lastVel = velocity;
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
		}
	}
}