using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TankBody : VehicleBody
{
	[SerializeField] int maxTurretHp = 5000;
	[SerializeField] int maxReloadHp = 5000;

	public event Action<float> OnTurretHpChanged;
	public event Action<float> OnReloadHpChanged;

	public float TurretRatio { get { return (float)CurTurretHp / maxTurretHp; } }
	public float ReloadRatio { get { return (float)CurReloadHp / maxReloadHp; } }

	[Networked, OnChangedRender(nameof(CurTurretChanged))] public int CurTurretHp { get; private set; }
	[Networked, OnChangedRender(nameof(CurReloadChanged))] public int CurReloadHp { get; private set; }

	public override void Spawned()
	{
		base.Spawned();
		if (HasStateAuthority)
		{
			CurTurretHp = maxTurretHp;
			CurReloadHp = maxReloadHp;
		}
		CurTurretChanged();
		CurReloadChanged();
	}

	private void CurTurretChanged()
	{
		OnTurretHpChanged?.Invoke(TurretRatio);
	}

	private void CurReloadChanged()
	{
		OnReloadHpChanged?.Invoke(ReloadRatio);
	}

	public override void ApplyDamage(Transform source, Vector3 point, Vector3 force, int damage)
	{
		base.ApplyDamage(source, point, force, damage);

	}
}