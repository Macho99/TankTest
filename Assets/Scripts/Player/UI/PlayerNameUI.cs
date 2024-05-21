using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerNameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshPro;


    public void Init(string nicname)
    {
        textMeshPro.text = nicname;
    }
    private void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);

    }

}
