using DistantLands.Cozy;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Unity.Collections.Unicode;

public class PlayerMainUI : SceneUI
{
    [SerializeField] private PlayerStatUI playerStatUI;
    [SerializeField] private MainWeaponUI mainWeaponUI;
    [SerializeField] TextMeshProUGUI timeText;
    int initTime;
    float startTime;

    public PlayerStatUI PlayerStatUI { get { return playerStatUI; } }
    public MainWeaponUI MainWeaponUI { get { return mainWeaponUI; } }

    public void UpdateStat(PlayerStatType statType, int currentValue, int maxValue)
    {
        playerStatUI.UpdateStat(statType, currentValue, maxValue);
    }
    public void ChangeWeaponUI(Weapon weapon, int totalAmmoCount = 1)
    {
        mainWeaponUI.ChangeWeaponUI(weapon, totalAmmoCount);
    }
    public void UpdateAmmo(Weapon weapon, int totalAmmoCount = 1)
    {
        mainWeaponUI.ChangeAmmoCount(weapon, totalAmmoCount);
    }
    public void SetInitTime(int initTime)
    {
        this.initTime = initTime;
        this.startTime = Time.time;
        StartCoroutine(CoUpdateTime());
    }

    IEnumerator CoUpdateTime()
    {
        while (true)
		{
            int curTime = initTime + (int)(Time.time - startTime);
			int minute = (curTime % 60);
			int hour = (curTime % 1440) / 60;
            timeText.text = $"{hour} : {minute}";

            yield return new WaitForSeconds(1f);
        }
    }
}
