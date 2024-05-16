using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatUI : MonoBehaviour
{

    [SerializeField] private PlayerStatDataUI[] playerStatDataUI;

    public void UpdateStat(PlayerStatType playerStatType, int currentValue, int maxValue)
    {
        playerStatDataUI[(int)playerStatType].UpdateData(currentValue, maxValue);
    }

}
[Serializable]
public class PlayerStatDataUI
{
    public PlayerStatType type;
    public TextMeshProUGUI GageTMP;
    public Image fillAmount;

    public void UpdateData(int currentValue, int maxValue)
    {
        GageTMP.text = $"{currentValue}/{maxValue}";
        fillAmount.fillAmount = currentValue / (float)maxValue;
    }
}