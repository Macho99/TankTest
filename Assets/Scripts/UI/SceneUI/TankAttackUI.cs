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
	[SerializeField] Color shellReloadColor;
	[SerializeField] Color shellReadyColor;

	Image[] shellImages = new Image[3];

	Image curAimImg;
	Image targetAimImg;

	Image reloadBar;
	TextMeshProUGUI smallReloadText;
	TextMeshProUGUI largeReloadText;

	protected override void Awake()
	{
		base.Awake();
		curAimImg = images["CurAimImg"];
		targetAimImg = images["TargetAimImg"];

		reloadBar = images["ReloadBar"];
		smallReloadText = texts["SmallReloadText"];
		largeReloadText = texts["LargeReloadText"];

		shellImages[0] = images["Shell0"];
		shellImages[1] = images["Shell1"];
		shellImages[2] = images["Shell2"];
	}

	public void Init(float finalAccuracy)
	{
		targetAimImg.rectTransform.localScale = Vector3.one * finalAccuracy;
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

	public void UpdateReloadUI(float smallTime, float shellRatio, int loadedShell, float largeTime, float barRatio, bool fireReady)
	{
		barRatio = Mathf.Clamp01(barRatio);
		barRatio = 1 - barRatio;
		barRatio *= 0.5f;
		Color color = fireReady ? readyColor : reloadColor;

		reloadBar.fillAmount = barRatio;
		this.smallReloadText.text = smallTime.ToString("F2");
		this.largeReloadText.text = largeTime.ToString("F2");
		reloadBar.color = color;
		this.largeReloadText.color = color;


	}
}
