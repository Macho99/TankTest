using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DashBoard : SceneUI
{
	Image rpmImage;
	TextMeshProUGUI velocityText;
	TextMeshProUGUI gearText;

	protected override void Awake()
	{
		base.Awake();
		rpmImage = images["RPMImage"];
		velocityText = texts["VelocityText"];
		gearText = texts["GearText"];
	}

	public void SetRPMAndVelUI(float rpm, float velocity)
	{    
		float startPos = 32f, endPos = -211f;
		float desiredPos = startPos - endPos;

		float temp = rpm / 10000;
		rpmImage.transform.eulerAngles = new Vector3(0, 0, (startPos - temp * desiredPos));

		velocityText.text = ((int)velocity).ToString();
	}

	public void SetGearUI(string gear)
	{
		gearText.text = gear;
	}
}
