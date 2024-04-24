using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AimUI : SceneUI
{
    [SerializeField] private GameObject detectRoot;
    [SerializeField] private TextMeshProUGUI interactDescription;


    protected override void Awake()
    {
        base.Awake();
        detectRoot.SetActive(false);
    }

    public void ReadInteractInfo(bool isDetect,InteractInfo info)
    {
        if (isDetect == false)
        {
            detectRoot.SetActive(false);
            return;
        }


        if (!detectRoot.activeSelf)
            detectRoot.SetActive(true);

        interactDescription.text = info.interactHint;
    }

}
