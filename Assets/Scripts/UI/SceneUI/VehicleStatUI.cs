using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VehicleStatUI : SceneUI
{
	TextMeshProUGUI vehicleNameText;
	TextMeshProUGUI vehicleHpText;
	Slider vehicleHpBar;
	Slider oilBar;
	Slider enginBar;

	Coroutine updateCoroutine;

	protected override void Awake()
	{
		base.Awake();
		vehicleNameText = texts["VehicleNameText"];
		vehicleHpBar = transforms["VehicleHpBar"].GetComponent<Slider>();
		vehicleHpText = texts["VehicleHpText"];
		oilBar = transforms["OilStatBar"].GetComponent<Slider>();
		enginBar = transforms["EngineStatBar"].GetComponent<Slider>();
	}

	public void Init(string vehicleName)
	{
		vehicleNameText.text = vehicleName;
	}

	public void UpdateHp(float ratio, int minHp, int maxHp)
	{
		vehicleHpBar.value = ratio;
		vehicleHpText.text = $"{minHp} / {maxHp}";
	}

	public void UpdateOil(float ratio)
	{
		oilBar.value = ratio;
	}

	public void UpdateEnginHp(float ratio)
	{
		enginBar.value = ratio;
	}
}
