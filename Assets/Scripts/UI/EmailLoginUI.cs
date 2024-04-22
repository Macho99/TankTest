using Firebase.Auth;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WebSocketSharp;

public class EmailLoginUI : PageUI
{
    [SerializeField] private TMP_InputField emailInputfield;
    [SerializeField] private TMP_InputField passwordInputfield;
    [SerializeField] private Toggle saveIDToggle;
    [SerializeField] private Toggle autoSaveToggle;

    public UnityEvent onSuccessLogin;
    private void Awake()
    {
        Debug.Log("awake");

    }

    public void Init(string email, bool isSaveID, bool isAutoLogin)
    {
        emailInputfield.text = email;
        saveIDToggle.isOn = isSaveID;
        autoSaveToggle.isOn = isAutoLogin;
        Debug.Log(isAutoLogin);
    }


    private void OnEnable()
    {

    }
    private void OnDisable()
    {
        if (!saveIDToggle.isOn)
            emailInputfield.text = string.Empty;

        passwordInputfield.text = string.Empty;
    }

    public async void PressLoginButton()
    {

        UIManagers uIManager = GameManagers.Instance.UIManager;
        if (emailInputfield.text == string.Empty || passwordInputfield.text == string.Empty)
        {
            uIManager.CreateMessageBoxUI("�α��� ����", "�Է�ĭ�� ��� �Է� �ϼž��մϴ�.", null);
            return;
        }

        uIManager.ActiveLoading(true, "�α��� �õ� ���Դϴ�.");

        AuthResult authResult = await GameManagers.Instance.AuthManager.LoginWithEmailAndPassword(emailInputfield.text, passwordInputfield.text);

        if (authResult != null)
        {
            if (!authResult.User.IsAnonymous)
                SaveLoginSetting();

            GameManagers.Instance.UIManager.ActiveLoading(true, "�κ� ������ �õ��ϰ��ֽ��ϴ�.");
            StartGameResult joinLobbyResult = await GameManagers.Instance.NetworkManager.JoinLobby();
            if (joinLobbyResult.Ok)
            {
                this.gameObject.SetActive(false);
                onSuccessLogin?.Invoke();
            }
            else
            {
                GameManagers.Instance.UIManager.CreateMessageBoxUI("�κ� ���� ����", "���� �� �� �����ϴ�.", null);
            }
            GameManagers.Instance.UIManager.ActiveLoading(false, null);


        }
        else
        {
            GameManagers.Instance.UIManager.CreateMessageBoxUI("�α��� ����", "�α��� ������ �����ʽ��ϴ�.", null);
        }





        uIManager.ActiveLoading(false, null);

    }
    public void SaveLoginSetting()
    {
        LoginSetting loginSetting = new LoginSetting();
        loginSetting.isSaveID = saveIDToggle.isOn;
        if (loginSetting.isSaveID == false)
            autoSaveToggle.isOn = false;
        else
            loginSetting.email = emailInputfield.text;

        loginSetting.isAutoSave = autoSaveToggle.isOn;
        LocalSaveLoader.SaveDataWithLocal("LoginSetting", loginSetting);
    }
    public void LoadLoginSetting()
    {
        if (LocalSaveLoader.LoadDataWithLocal("LoginSetting", out LoginSetting data))
        {
            Init(data.email, data.isSaveID, data.isAutoSave);
        }
    }
}

public class LoginSetting
{
    public string email;
    public bool isSaveID;
    public bool isAutoSave;
    public LoginSetting()
    {

    }
    public LoginSetting(string email, bool isSaveID, bool isAutoSave)
    {
        this.email = email;
        this.isSaveID = isSaveID;
        this.isAutoSave = isAutoSave;
    }
}