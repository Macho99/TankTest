using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TankWrecked : VehicleWrecked
{
	[SerializeField] VehicleWrecked turretPrefab;
	[SerializeField] Transform turretTrans;

	public override void Spawned()
	{
		base.Spawned();
		if (HasStateAuthority)
		{
			Runner.Spawn(turretPrefab, turretTrans.position, turretTrans.rotation);
		}
	}

	protected override void ExplosionForce()
	{
		//base.ExplosionForce();
	}

	protected override void PlayExplosionVfx()
	{
		//base.PlayExplosionVfx();
	}
}