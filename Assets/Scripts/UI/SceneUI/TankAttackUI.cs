using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TankAttackUI : SceneUI
{
	[SerializeField] Color readyColor;
	[SerializeField] Color reloadColor;

	Image curAimImg;
	Image targetAimImg;

	Image reloadBar;
	TextMeshProUGUI reloadText;

	float reloadTime;
	float ratioMultiplier;

	protected override void Awake()
	{
		base.Awake();
		curAimImg = images["CurAimImg"];
		targetAimImg = images["TargetAimImg"];

		reloadBar = images["ReloadBar"];
		reloadText = texts["ReloadText"];
	}

	public void Init(float finalAccuracy, float reloadTime)
	{
		targetAimImg.rectTransform.localScale = Vector3.one * finalAccuracy;
		this.reloadTime = reloadTime;
		ratioMultiplier = 1f / reloadTime;
	}

	public void UpdateAimUI(Vector3 screenPos, float accuracy)
	{
		if (screenPos.z < 0f)
		{
			curAimImg.rectTransform.position = new Vector3(-2000, -2000, 0f);
		}
		else
		{
			curAimImg.rectTransform.position = screenPos;// - centerPos;
		}
		curAimImg.rectTransform.localScale = Vector3.one * accuracy;
	}

	public void UpdateReloadUI(float? remainNullableTime)
	{
		Color color;
		if(remainNullableTime.HasValue == true)
		{
			float remainTime = remainNullableTime.Value;
			if(remainTime > 0f)
			{
				reloadBar.fillAmount = (reloadTime - remainTime) * ratioMultiplier * 0.5f;
				reloadText.text = remainTime.ToString("F2");
				color = reloadColor;
			}
			else
			{
				reloadBar.fillAmount = 0.5f;
				reloadText.text = reloadTime.ToString("F2");
				color = readyColor;
			}
		}
		else
		{
			reloadBar.fillAmount = 0.5f;
			reloadText.text = reloadTime.ToString("F2");
			color = readyColor;
		}
		reloadBar.color = color;
		reloadText.color = color;
	}
}
