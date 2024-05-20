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

    public void CreateSessionItem(SessionInfo newSessionInfo, Action<SessionInfo> sessionJoin, PrivateSessionConnetUI privateSessionConnetUI = null)
    {
        if (newSessionInfo == null)
        {
            Destroy(gameObject);
            onSessionJoin -= sessionJoin;
            return;
        }

        this.sessionInfo = newSessionInfo;
        print(this.sessionInfo.Properties);
        sessionNameTMP.text = sessionInfo.Name;
        sessionCountTMP.text = $"({sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers})";
        this.privateSessionConnetUI = privateSessionConnetUI;

        sessionLockImage.enabled = sessionInfo.Properties.ContainsKey("Password");
        sessionJoinButton.interactable = sessionInfo.PlayerCount < sessionInfo.MaxPlayers ? true : false;
        onSessionJoin += sessionJoin;
    }
    public async void PressJoinSessionButton()
    {

        if (sessionInfo.Properties.ContainsKey("Password"))
        {
            privateSessionConnetUI.ActivePrivateSeesionConneter(sessionInfo, onSessionJoin);
            Debug.Log("asdasd");
        }
        else
        {
            LoadingUI loadingUI = GameManager.UI.ShowPopUpUI<LoadingUI>("UI/LoadingUI");
            loadingUI.Init("게임 방에 입장중 입니다.");
            StartGameResult result = await GameManager.network.JoinSession(sessionInfo);
            if (result != null)
            {

                onSessionJoin?.Invoke(sessionInfo);
            }
            loadingUI.CloseUI();
        }




    }

}
