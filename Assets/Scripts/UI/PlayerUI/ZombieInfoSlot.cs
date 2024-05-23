using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ZombieInfoSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countTMP;

    public void UpdateSlot(int count)
    {
        countTMP.text = count.ToString();
    }
}
