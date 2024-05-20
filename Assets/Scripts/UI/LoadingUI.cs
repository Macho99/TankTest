using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingUI : PopUpUI
{
    [SerializeField] private TextMeshProUGUI contentText;



    public void Init(string content)
    {
        if (content == null)
        {
            contentText.text = string.Empty;
            return;
        }
        contentText.text = content;
    }

}
