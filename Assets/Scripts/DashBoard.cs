using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashBoard : MonoBehaviour
{
	[SerializeField] Image rpmNeedle;
	[SerializeField] Text velocityText;
	[SerializeField] Text gearText;

	public void SetRPMAndVelUI(float rpm, float velocity)
	{    
		float startPos = 32f, endPos = -211f;
		float desiredPos = startPos - endPos;

		float temp = rpm / 10000;
		rpmNeedle.transform.eulerAngles = new Vector3(0, 0, (startPos - temp * desiredPos));

		velocityText.text = ((int)velocity).ToString();
	}

	public void SetGearUI(string gear)
	{
		gearText.text = gear;
	}
}
