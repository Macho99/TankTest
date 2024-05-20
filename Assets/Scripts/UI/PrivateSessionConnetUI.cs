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
    public void ActivePrivateSeesionConneter(SessionInfo sessionInfo, Action<SessionInfo> joinAction)
    {
        this.sessionInfo = sessionInfo;
        this.onJoin = joinAction;
        gameObject.SetActive(true);
    }
    public async void PressJoinButton()
    {

        if ((string)sessionInfo.Properties["Password"].PropertyValue == passwordField.text)
        {
            LoadingUI loadingUI = GameManager.UI.ShowPopUpUI<LoadingUI>("UI/LoadingUI");
            loadingUI.Init("���� �濡 ������ �Դϴ�.");
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

            GameManager.UI.ShowPopUpUI<MessageBoxUI>("UI/MessageBoxUI").Init("�� ���� ����", "��й�ȣ�� ���� �ʽ��ϴ�.", null);
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
