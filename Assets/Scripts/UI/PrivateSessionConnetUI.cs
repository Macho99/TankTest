using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PrivateSessionConnetUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField passwordField;
    private SessionInfo sessionInfo;
    private Action<SessionInfo> onJoin;
    private LoadingUI loadingUIPrefab;
    private MessageBoxUI messageBoxUIPrefab;
    private void Awake()
    {
        loadingUIPrefab = GameManager.Resource.Load<LoadingUI>("UI/LoadingUI");
        messageBoxUIPrefab = GameManager.Resource.Load<MessageBoxUI>("UI/MessageBoxUI");
    }
    public void ActivePrivateSeesionConneter(SessionInfo sessionInfo, Action<SessionInfo> joinAction =null)
    {
        this.sessionInfo = sessionInfo;
        this.onJoin = joinAction;
        gameObject.SetActive(true);
    }
    public async void PressJoinButton()
    {

        if ((string)sessionInfo.Properties["Password"].PropertyValue == passwordField.text)
        {
            LoadingUI loadingUI = GameManager.UI.ShowPopUpUI(loadingUIPrefab);
            loadingUI.Init("게임 방에 입장중 입니다.");
            StartGameResult result = await GameManager.network.JoinSession(sessionInfo);
            if (result != null)
            {
                onJoin?.Invoke(sessionInfo);
                DisableConnetUI();
            }
            loadingUI.CloseUI();
        }
        else
        {

            GameManager.UI.ShowPopUpUI(messageBoxUIPrefab).Init("방 접속 실패", "비밀번호가 맞지 않습니다.", null);
        }
    }
    public void PressCancelButton()
    {
        DisableConnetUI();
    }
    private void DisableConnetUI()
    {
        passwordField.text = string.Empty;
        sessionInfo = null;
        onJoin = null;
        transform.gameObject.SetActive(false);
    }
}
