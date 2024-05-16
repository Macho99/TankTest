using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMainUI : SceneUI
{
    [SerializeField] private PlayerStatUI playerStatUI;
    [SerializeField] private MainWeaponUI mainWeaponUI;

    public PlayerStatUI PlayerStatUI { get { return playerStatUI; } }
    public MainWeaponUI MainWeaponUI { get { return mainWeaponUI; } }

    public void UpdateStat(PlayerStatType statType, int currentValue, int maxValue)
    {
        playerStatUI.UpdateStat(statType, currentValue, maxValue);
    }
    public void ChangeWeaponUI(Weapon weapon)
    {
        mainWeaponUI.ChangeWeaponUI(weapon);
    }
    public void UpdateAmmo(Weapon weapon)
    {
        mainWeaponUI.ChangeAmmoCount(weapon);
    }
}
