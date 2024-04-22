using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SessionItem : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI sessionNameTMP;
    [SerializeField] private TextMeshProUGUI sessionCountTMP;
    [SerializeField] private Image sessionLockImage;
    [SerializeField] private Button sessionJoinButton;
    private SessionInfo sessionInfo;
    private Action<SessionInfo> onSessionJoin;
    private PrivateSessionConnetUI privateSessionConnetUI;

    public void CreateSessionItem(SessionInfo sessionInfo, Action<SessionInfo> sessionJoin, PrivateSessionConnetUI privateSessionConnetUI = null)
    {
        if (sessionInfo == null)
        {
            Destroy(gameObject);
            onSessionJoin -= sessionJoin;
            return;
        }

        this.sessionInfo = sessionInfo;

        sessionNameTMP.text = sessionInfo.Name;
        sessionCountTMP.text = $"({sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers})";
        this.privateSessionConnetUI = privateSessionConnetUI;
        sessionLockImage.enabled = sessionInfo.Properties["Password"] != null;
        sessionJoinButton.interactable = sessionInfo.PlayerCount < sessionInfo.MaxPlayers ? true : false;
        onSessionJoin += sessionJoin;
    }
    public async void PressJoinSessionButton()
    {

        if (sessionInfo.Properties["Password"] != null)
        {
            privateSessionConnetUI.ActivePrivateSeesionConneter(sessionInfo, onSessionJoin);
            Debug.Log("asdasd");
        }
        else
        {
            GameManagers.Instance.UIManager.ActiveLoading(true, "게임 방에 입장중 입니다.");
            StartGameResult result = await GameManagers.Instance.NetworkManager.JoinSession(sessionInfo);
            if (result != null)
            {

                onSessionJoin?.Invoke(sessionInfo);
            }
            GameManagers.Instance.UIManager.ActiveLoading(false);
        }




    }

}
