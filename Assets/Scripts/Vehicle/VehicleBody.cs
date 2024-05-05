using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class VehicleBody : NetworkBehaviour, IHittable
{
	private struct HitData
	{
		public uint id;
		public TickTimer nextHitTimer;
	}

	[SerializeField] BoxCollider vehicleTrigger;
	[SerializeField] int maxHp = 10000;
	[SerializeField] int damage = 10;
	[SerializeField] float hitCooltime = 0.5f;

	Collider[] cols = new Collider[30];

	int curHp;
	Rigidbody rb;
	List<HitData> hitDataList = new List<HitData>();

	public LayerMask BodyAttackMask { get; private set; }
	public int MonsterLayer { get; private set; }

	public uint HitID => Object.Id.Raw;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		MonsterLayer = LayerMask.NameToLayer("Monster");
		BodyAttackMask = LayerMask.GetMask("Monster", "Breakable");
	}

	public override void FixedUpdateNetwork()
	{
		if (rb.velocity.sqrMagnitude < 10f)
		{
			if(hitDataList.Count > 0)
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
				Hit(hittable, cols[i].transform);
			}
			else
			{
				HitData hitData = hitDataList[idx];
				if(hitData.nextHitTimer.ExpiredOrNotRunning(Runner) == true)
				{
					hitData.nextHitTimer = TickTimer.CreateFromSeconds(Runner, hitCooltime);
					hitDataList[idx] = hitData;
					Hit(hittable, cols[i].transform);
				}
			}
		}
	}

	private void Hit(IHittable hittable, Transform colTrans)
	{
		//print($"{colTrans.name} 때림");
		hittable.ApplyDamage(transform, transform.position,
			(colTrans.position - transform.position).normalized * rb.velocity.magnitude * 20f, (int) (damage * rb.velocity.magnitude));
	}

	public void ApplyDamage(Transform source, Vector3 point, Vector3 force, int damage)
	{

	}
}