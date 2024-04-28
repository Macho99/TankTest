using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OperationUI : InGameUI
{
    [SerializeField] private RectTransform progressRT;
    [SerializeField] private TextMeshProUGUI percentTMP;

    private float rotateSpeed;
    protected override void Init()
    {
        rotateSpeed = 360f;
        gameObject.SetActive(false);
    }

    public void SetProgress(float percent)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
           
        }
        
        progressRT.Rotate(new Vector3(0f, 0f, rotateSpeed * Time.deltaTime));
        percentTMP.text = $"{percent}%";
    }
    //public override void CloseUI()
    //{
    //    this.gameObject.SetActive(false);
    //}

}
