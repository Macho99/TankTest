using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TankStatUI : VehicleStatUI
{
	Slider turretBar;
	Slider reloadBar;
	TankBody tankBody;

	protected override void Awake()
	{
		base.Awake();
		turretBar = transforms["TurretStatBar"].GetComponent<Slider>();
		reloadBar = transforms["ReloadStatBar"].GetComponent<Slider>();
	}

	public override void Init(string vehicleName, VehicleBody vehicleBody)
	{
		base.Init(vehicleName, vehicleBody);
		tankBody = vehicleBody as TankBody;
		if (tankBody == null)
		{
			Debug.LogError($"{vehicleBody.name}를 TankBody 컴포넌트로 바꿔주세요");
		}
	}

	public override void AddEvents()
	{
		base.AddEvents();
		tankBody.OnTurretHpChanged += UpdateTurretBar;
		tankBody.OnReloadHpChanged += UpdateReloadBar;

		UpdateTurretBar(tankBody.TurretRatio);
		UpdateReloadBar(tankBody.ReloadRatio);
	}

	public override void RemoveEvents()
	{
		base.RemoveEvents();
		tankBody.OnTurretHpChanged -= UpdateTurretBar;
		tankBody.OnReloadHpChanged -= UpdateReloadBar;
	}

	public void UpdateTurretBar(float ratio)
	{
		turretBar.value = ratio;
	}

	public void UpdateReloadBar(float ratio)
	{
		reloadBar.value = ratio;
	}
}