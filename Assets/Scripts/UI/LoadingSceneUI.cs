using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSceneUI : PopUpUI
{
    [SerializeField] private Image loadingFill;
    [SerializeField] private TextMeshProUGUI loadingProgressTMP;


    public void ProgressLoading(float value)
    {
        loadingFill.fillAmount = value;
        loadingProgressTMP.text = $"{value * 100}%";
    }
}
