using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameMenuUI : PopUpUI
{
    [SerializeField] private GameObject menuPanel;



    public void PressSettingButton()
    {

    }
    public void PressExitButton()
    {
        Application.Quit();

    }
    public void BackButton()
    {
        CloseUI();
    }
}
