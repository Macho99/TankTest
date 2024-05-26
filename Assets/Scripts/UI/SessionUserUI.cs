using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionUserUI : MonoBehaviour
{
    [SerializeField] private Image hostMarkImage;
    [SerializeField] private TextMeshProUGUI nicnameTMP;
    [SerializeField] private TextMeshProUGUI readyTMP;

    public void Setup(RoomPlayer player, PlayerRef playerRef, bool isReady)
    {
        nicnameTMP.text = player.UserName;
        hostMarkImage.enabled = player.isHost;
        readyTMP.enabled = isReady;

    }
    public void UpdateNicname(string name,PlayerRef playerRef)
    {
        nicnameTMP.text = name;
    }
    public void ActiveReady(bool isReady)
    {
        readyTMP.enabled = isReady;
    }


}
