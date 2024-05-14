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

	int lastLargeTimeInt;
	int loadedShell;
	Image[] shellImages = new Image[3];
	Animator anim;

	Image curAimImg;
	Image targetAimImg;
	RectTransform targetAimScaleTrans;

	Image reloadBar;
	TextMeshProUGUI smallReloadText;
	TextMeshProUGUI largeReloadText;

	protected override void Awake()
	{
		base.Awake();
		curAimImg = images["CurAimImg"];
		targetAimScaleTrans = transforms["TargetAimScale"];
		targetAimImg = images["TargetAimImg"];

		reloadBar = images["ReloadBar"];
		smallReloadText = texts["SmallReloadText"];
		largeReloadText = texts["LargeReloadText"];

		shellImages[0] = images["Shell0"];
		shellImages[1] = images["Shell1"];
		shellImages[2] = images["Shell2"];


		for (int i = 0; i < shellImages.Length; i++)
		{
			Image image = shellImages[i];
			image.fillAmount = 0f;
			image.color = shellReloadColor;
		}
		anim = GetComponent<Animator>();
	}

	private void OnEnable()
	{
		targetAimImg.rectTransform.localRotation = Quaternion.identity;
	}

	public void Init(float finalAccuracy, int loadedShell)
	{
		this.loadedShell = loadedShell;
		for (int i = 0; i < shellImages.Length; i++)
		{
			Image image = shellImages[i];
			if (i < loadedShell)
			{
				image.color = shellReadyColor;
				image.fillAmount = 1f;
			}
			else
			{
				image.color = shellReloadColor;
				image.fillAmount = 0f;
			}
		}

		targetAimScaleTrans.localScale = Vector3.one * finalAccuracy;
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

	public void UpdateReloadUI(float smallTime, float shellRatio, float largeTime, float barRatio, bool fireReady)
	{
		barRatio = Mathf.Clamp01(barRatio);
		barRatio = 1f - barRatio;
		barRatio *= 0.5f;
		Color color = fireReady ? readyColor : reloadColor;

		reloadBar.fillAmount = barRatio;
		this.smallReloadText.text = smallTime.ToString("F1");
		this.largeReloadText.text = largeTime.ToString("F1");
		reloadBar.color = color;
		this.largeReloadText.color = color;

		if(loadedShell < shellImages.Length)
		{
			shellImages[loadedShell].fillAmount = 1f - shellRatio;
		}

		if(fireReady == false)
		{
			int curLargetTimeInt = (int)largeTime;
			if(lastLargeTimeInt != curLargetTimeInt)
			{
				lastLargeTimeInt = curLargetTimeInt;
				if(largeTime - curLargetTimeInt > 0.9f)
				{
					anim.SetTrigger("AimRotate");
				}
			}
		}
	}

	public void Fired()
	{
		anim.SetTrigger("Fire");
	}

	public void Reloaded()
	{
		shellImages[loadedShell].fillAmount = 1f;
		shellImages[loadedShell].color = shellReadyColor;
		loadedShell++;
	}

	private void FirstShellSwitched()
	{
		shellImages[0].fillAmount = 0f;
		shellImages[0].color = shellReloadColor;
	}

	private void FireEnd()
	{
		for (int i = 0; i < shellImages.Length - 1; i++)
		{
			prevOverwrite(shellImages[i], shellImages[i + 1]);
		}
		shellImages[shellImages.Length - 1].fillAmount = 0f;
		shellImages[shellImages.Length - 1].color = shellReloadColor;

		loadedShell--;
	}

	private void prevOverwrite(Image prev, Image next)
	{
		prev.color = next.color;
		prev.fillAmount = next.fillAmount;
	}
}