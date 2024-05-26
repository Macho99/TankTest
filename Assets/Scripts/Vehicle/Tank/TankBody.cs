using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class TankBody : VehicleBody
{
	[SerializeField] int maxTurretHp = 5000;
	[SerializeField] int maxReloadHp = 5000;

	public event Action<float> OnTurretHpChanged;
	public event Action<float> OnReloadHpChanged;

	public float TurretRatio { get { return (float)CurTurretHp / maxTurretHp; } }
	public float ReloadRatio { get { return (float)CurReloadHp / maxReloadHp; } }

	[Networked, OnChangedRender(nameof(CurTurretChanged)), HideInInspector] 
	public int CurTurretHp { get; private set; }

	[Networked, OnChangedRender(nameof(CurReloadChanged)), HideInInspector] 
	public int CurReloadHp { get; private set; }

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

	protected override void CheckModuleDamaged(Vector3 diff, float fwdAngle, float upAngle, int damage)
	{
		//base.CheckModuleDamaged(diff, fwdAngle, upAngle, damage);
		//전차 엔진은 뒤에 있음
		print($"{fwdAngle}, {upAngle}, {damage}");
		if (fwdAngle > 160f)
		{
			float engineRatio = Random.value;
			CurEngineHp = Mathf.Max(CurEngineHp - (int)(engineRatio * damage), 0);
		}
		//위쪽에 맞으면 포탑과 장전 패널티
		if(upAngle < 45f)
		{
			float turretRatio = Random.value;
			CurTurretHp = Mathf.Max(CurTurretHp - (int)(turretRatio * damage), 0);

			float reloadRatio = Random.value;
			CurReloadHp = Mathf.Max(CurReloadHp - (int)(reloadRatio * damage), 0);
		}
		else
		{
			//데미지가 충분히 셀 경우 확률적으로 패널티
			if (damage > 800)
			{
				if(Random.value > 0.7f)
				{
					float turretRatio = Random.value;
					CurTurretHp = Mathf.Max(CurTurretHp - (int)(turretRatio * damage), 0);
				}
				if(Random.value > 0.7f)
				{
					float reloadRatio = Random.value;
					CurReloadHp = Mathf.Max(CurReloadHp - (int)(reloadRatio * damage), 0);
				}
			}
		}
	}
}