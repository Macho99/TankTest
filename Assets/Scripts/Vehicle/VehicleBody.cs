using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class VehicleBody : NetworkBehaviour, IHittable
{
	const float hitCooltime = 1f;
	const float COR = 1f; //반발계수

	private struct HitData
	{
		public Int64 id;
		public TickTimer nextHitTimer;
	}

	[SerializeField] BoxCollider vehicleTrigger;
	[SerializeField] int maxHp = 10000;
	[SerializeField] int maxOil = 5000;
	[SerializeField] int maxEngineHp = 5000;
	[SerializeField] int damage = 50;

	LayerMask collisionMask;
	Collider[] cols = new Collider[30];
	Rigidbody rb;
	List<HitData> hitDataList = new List<HitData>();
	TickTimer lastHitTimer;
	float massRatio;

	public LayerMask BodyAttackMask { get; private set; }
	public int MonsterLayer { get; private set; }
	public int BreakableLayer { get; private set; }

	public Int64 HitID => (Object.Id.Raw << 32);

	public event Action<float> OnCurHpChanged;
	public event Action<float> OnOilChanged;
	public event Action<float> OnCurEnginHpChanged;

	public int MaxHp { get { return maxHp; } }
	public float HpRatio { get { return (float)CurHp / maxHp; } }
	public float OilRatio { get { return (float)CurOil / maxOil; } }
	public float EngineHpRatio { get { return (float)CurEngineHp / maxEngineHp; } }
	[Networked, OnChangedRender(nameof(CurHpChanged))] public int CurHp { get; private set; }
	[Networked, OnChangedRender(nameof(CurOilChanged))] public int CurOil { get; private set; }
	[Networked, OnChangedRender(nameof(CurEngineHpChanged))] public int CurEngineHp { get; protected set; }

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		massRatio = rb.mass / 1500f;
		MonsterLayer = LayerMask.NameToLayer("Monster");
		BreakableLayer = LayerMask.NameToLayer("Breakable");
		BodyAttackMask = LayerMask.GetMask("Monster", "Breakable");
		collisionMask = LayerMask.GetMask("Environment", "Vehicle");
	}

	public override void Spawned()
	{
		if (HasStateAuthority)
		{
			CurHp = maxHp;
			CurOil = maxOil;
			CurEngineHp = maxEngineHp;
		}
		CurHpChanged();
		CurOilChanged();
		CurEngineHpChanged();
	}

	public void ConsumpOil(int amount)
	{
		CurOil -= amount;
		if(CurOil < 0)
		{
			CurOil = 0;
		}
	}

	public override void FixedUpdateNetwork()
	{
		if (rb.velocity.sqrMagnitude < 10f)
		{
			if(hitDataList.Count > 0 && lastHitTimer.ExpiredOrNotRunning(Runner))
			{
				hitDataList.Clear();
			}
			return;
		}

		int result = Physics.OverlapBoxNonAlloc(vehicleTrigger.center + transform.position, 
			vehicleTrigger.size * 0.5f, cols, transform.rotation, BodyAttackMask);
		for (int i = 0; i < result; i++)
		{
			IHittable hittable = cols[i].GetComponent<IHittable>();
			if (hittable == null) continue;

			int idx = hitDataList.FindIndex((x) => x.id == hittable.HitID);
			if (idx == -1)
			{
				hitDataList.Add(new HitData() { id = hittable.HitID, nextHitTimer = TickTimer.CreateFromSeconds(Runner, hitCooltime) });
				Hit(hittable, cols[i].gameObject);
			}
			else
			{
				HitData hitData = hitDataList[idx];
				if(hitData.nextHitTimer.ExpiredOrNotRunning(Runner) == true)
				{
					hitData.nextHitTimer = TickTimer.CreateFromSeconds(Runner, hitCooltime);
					hitDataList[idx] = hitData;
					Hit(hittable, cols[i].gameObject);
				}
			}
		}
	}

	private void Hit(IHittable hittable, GameObject other)
	{
		lastHitTimer = TickTimer.CreateFromSeconds(Runner, 5f);
		Vector3 normal = (other.transform.position - transform.position).normalized;
		Vector3 force = Vector3.zero;
		float velMag = rb.velocity.magnitude;
		int finaldamage = (int)(damage * velMag);
		if (other.layer == MonsterLayer)
		{
			ZombieBase zombieBase = other.GetComponent<ZombieHitBox>().Owner;
			force = CalcForce(zombieBase.Mass, normal, COR);
			if (force == Vector3.zero)
				return;
			hittable.ApplyDamage(transform, transform.position, force, finaldamage);
			//print($"{other.name}: {finaldamage}");
		}
		else if(other.layer == BreakableLayer)
		{
			hittable.ApplyDamage(transform, transform.position,
				normal * velMag * 5f, (int) (finaldamage * massRatio));
			//print($"{other.name}: {(int)(finaldamage * massRatio)}");
		}
		this.ApplyDamage(transform, other.transform.position, force * 1.5f, finaldamage / 5);
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (HasStateAuthority && collisionMask.IsLayerInMask(collision.collider.gameObject.layer))
		{
			StartCoroutine(CoCheckCollision(collision.GetContact(0).point, rb.velocity));
		}
	}

	private IEnumerator CoCheckCollision(Vector3 point, Vector3 prevVelocity)
	{
		for(int i = 0; i < 2; i++)
		{
			yield return new WaitForFixedUpdate();
		}
		Vector3 velDiff = rb.velocity - prevVelocity;
		//print($"{velDiff}");
		int finaldamage = (int)(damage * velDiff.magnitude);
		this.ApplyDamage(transform, point, Vector3.zero, finaldamage);
	}

	private Vector3 CalcForce(float otherMass, Vector3 normal, float cor)
	{
		//print((1 + COR) * Vector3.Dot(rb.velocity, normal) / ((1 / rb.mass) + (1 / otherMass)));
		return (1 + cor) * Mathf.Max(Vector3.Dot(rb.velocity, normal), 0f) / ((1 / rb.mass) + (1 / otherMass)) * normal;
	}

	protected void CurHpChanged()
	{
		OnCurHpChanged?.Invoke(HpRatio);
	}

	protected void CurOilChanged()
	{
		OnOilChanged?.Invoke(OilRatio);
	}
	
	protected void CurEngineHpChanged()
	{
		OnCurEnginHpChanged(EngineHpRatio);
	}

	public virtual void ApplyDamage(Transform source, Vector3 point, Vector3 force, int damage)
	{
		CurHp = Mathf.Max(CurHp - damage, 0);

		rb.AddForce(force, ForceMode.Impulse);
		Vector3 diff = point - transform.position;
		float fwdAngle = Vector3.Angle(diff, transform.forward);
		float upAngle = Vector3.Angle(diff, transform.up);

		Random.InitState(Runner.Tick * unchecked((int)Object.Id.Raw));
		CheckModuleDamaged(diff, fwdAngle, upAngle, damage);
	}

	protected virtual void CheckModuleDamaged(Vector3 diff, float fwdAngle, float upAngle, int damage)
	{
		if(fwdAngle < 20f)
		{
			float engineRatio = Random.value;
			CurEngineHp = Mathf.Max(CurEngineHp - (int) (engineRatio * damage), 0);
		}
	}
}