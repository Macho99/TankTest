using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMainUI : SceneUI
{
    [SerializeField] private PlayerStatUI playerStatUI;


    public void UpdateStat(PlayerStatType statType, int currentValue, int maxValue)
    {
        playerStatUI.UpdateStat(statType, currentValue, maxValue);
    }
}
