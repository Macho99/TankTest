using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugLogTextItem : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI debugKey;
    [SerializeField] private TextMeshProUGUI debugValue;


    public void SetText(string Key, string Value)
    {
        debugKey.text = Key;
        debugValue.text = $" : {Value}" ;



    }

}
