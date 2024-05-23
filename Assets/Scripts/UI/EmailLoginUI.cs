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
    private LoadingUI loadingUIPrefab;
    private MessageBoxUI messageBoxUIPrefab;
    public UnityEvent onSuccessLogin;
    [SerializeField] private AudioClip buttonClickSFX;
    private void Awake()
    {
        Debug.Log("awake");
        loadingUIPrefab = GameManager.Resource.Load<LoadingUI>("UI/LoadingUI");
        messageBoxUIPrefab = GameManager.Resource.Load<MessageBoxUI>("UI/MessageBoxUI");
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
        UIManager uIManager = GameManager.UI;
        if (emailInputfield.text == string.Empty || passwordInputfield.text == string.Empty)
        {
            GameManager.UI.ShowPopUpUI(messageBoxUIPrefab).Init("�α��� ����", "�Է�ĭ�� ��� �Է� �ϼž��մϴ�.", null);
            return;
        }

        LoadingUI loadingUI = GameManager.UI.ShowPopUpUI(loadingUIPrefab);
        loadingUI.Init("�α��� �õ� ���Դϴ�.");

        AuthResult authResult = await GameManager.auth.LoginWithEmailAndPassword(emailInputfield.text, passwordInputfield.text);

        if (authResult != null)
        {
            if (!authResult.User.IsAnonymous)
                SaveLoginSetting();

            loadingUI.Init("�κ� ������ �õ��ϰ��ֽ��ϴ�.");
            Debug.LogWarning("networkmanagersutdown");
            StartGameResult joinLobbyResult = await GameManager.network.JoinLobby();
            if (joinLobbyResult.Ok)
            {
                this.gameObject.SetActive(false);
                onSuccessLogin?.Invoke();
            }
            else
            {
                GameManager.UI.ShowPopUpUI(messageBoxUIPrefab).Init("�κ� ���� ����", "���� �� �� �����ϴ�.", null);
            }
            loadingUI.CloseUI();

            return;
        }
        else
        {
            GameManager.UI.ShowPopUpUI(messageBoxUIPrefab).Init("�α��� ����", "�α��� ������ �����ʽ��ϴ�.", null);
        }





        loadingUI.CloseUI();

    }
    public void PressButton()
    {
        GameManager.Sound.PlayOneShot(buttonClickSFX, AudioGroup.SFX, Camera.main.transform, false);
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