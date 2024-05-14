using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VehicleStatUI : SceneUI
{
	VehicleBody vehicleBody;

	TextMeshProUGUI vehicleNameText;
	TextMeshProUGUI vehicleHpText;
	Slider vehicleHpBar;
	Slider oilBar;
	Slider engineBar;

	Coroutine updateCoroutine;

	protected override void Awake()
	{
		base.Awake();
		vehicleNameText = texts["VehicleNameText"];
		vehicleHpBar = transforms["VehicleHpBar"].GetComponent<Slider>();
		vehicleHpText = texts["VehicleHpText"];
		oilBar = transforms["OilStatBar"].GetComponent<Slider>();
		engineBar = transforms["EngineStatBar"].GetComponent<Slider>();
	}

	public virtual void Init(string vehicleName, VehicleBody vehicleBody)
	{
		vehicleNameText.text = vehicleName;
		this.vehicleBody = vehicleBody;
	}

	public virtual void AddEvents()
	{
		vehicleBody.OnCurHpChanged += UpdateHp;
		vehicleBody.OnOilChanged += UpdateOil;
		vehicleBody.OnCurEnginHpChanged += UpdateEnginHp;

		UpdateHp(vehicleBody.HpRatio);
		UpdateOil(vehicleBody.OilRatio);
		UpdateEnginHp(vehicleBody.EngineHpRatio);
	}

	public virtual void RemoveEvents()
	{
		vehicleBody.OnCurHpChanged -= UpdateHp;
		vehicleBody.OnOilChanged -= UpdateOil;
		vehicleBody.OnCurEnginHpChanged -= UpdateEnginHp;
	}

	public void UpdateHp(float ratio)
	{
		vehicleHpBar.value = ratio;
		vehicleHpText.text = $"{vehicleBody.CurHp} / {vehicleBody.MaxHp}";
	}

	public void UpdateOil(float ratio)
	{
		oilBar.value = ratio;
	}

	public void UpdateEnginHp(float ratio)
	{
		engineBar.value = ratio;
	}
}
