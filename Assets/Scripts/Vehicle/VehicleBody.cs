using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class VehicleBody : ExplosiveBody
{
	[SerializeField] int maxOil = 5000;
	[SerializeField] int maxEngineHp = 5000;

	public event Action<float> OnOilChanged;
	public event Action<float> OnCurEnginHpChanged;

	public float OilRatio { get { return (float)CurOil / maxOil; } }
	public float EngineHpRatio { get { return (float)CurEngineHp / maxEngineHp; } }

	[Networked, OnChangedRender(nameof(CurOilChanged)), HideInInspector] 
	public int CurOil { get; private set; }

	[Networked, OnChangedRender(nameof(CurEngineHpChanged)), HideInInspector]
	public int CurEngineHp { get; protected set; }

	public override void Spawned()
	{
		base.Spawned();
		if (HasStateAuthority)
		{
			CurOil = maxOil;
			CurEngineHp = maxEngineHp;
		}
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

	protected void CurOilChanged()
	{
		OnOilChanged?.Invoke(OilRatio);
	}
	
	protected void CurEngineHpChanged()
	{
		OnCurEnginHpChanged(EngineHpRatio);
	}

	protected override void CheckModuleDamaged(Vector3 diff, float fwdAngle, float upAngle, int damage)
	{
		base.CheckModuleDamaged(diff, fwdAngle, upAngle, damage);
		if(fwdAngle < 20f)
		{
			float engineRatio = Random.value;
			CurEngineHp = Mathf.Max(CurEngineHp - (int) (engineRatio * damage), 0);
		}
	}
}