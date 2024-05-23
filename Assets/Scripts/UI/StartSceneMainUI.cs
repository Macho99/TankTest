using Firebase.Auth;
using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class StartSceneMainUI : MonoBehaviour
{
    [SerializeField] private AuthUI authUI;
    [SerializeField] private LobbyUI lobbyUI;
    private MessageBoxUI messageBoxUIPrefab;
    private LoadingUI loadingUIPrefab;

    Action action;
    // Start is called before the first frame update
    void Start()
    {
        messageBoxUIPrefab = GameManager.Resource.Load<MessageBoxUI>("UI/MessageBoxUI");
        MessageBoxUI messageBoxUI = GameManager.UI.ShowPopUpUI(messageBoxUIPrefab);
        messageBoxUI.CloseUI();
        loadingUIPrefab = GameManager.Resource.Load<LoadingUI>("UI/LoadingUI");
        LoadingUI loadingUI = GameManager.UI.ShowPopUpUI(loadingUIPrefab);
        loadingUI.CloseUI();
        Initialized();
    }
    public void Initialized()
    {

        AuthManager authManager = GameManager.auth;


        if (authManager.User != null)
        {
            if (!authManager.User.IsAnonymous)
            {
                if (LocalSaveLoader.LoadDataWithLocal("LoginSetting", out LoginSetting setting))
                {
                    if (setting.isAutoSave)
                    {
                        authUI.ExistLoginInfo(true);
                        if (GameManager.network.Runner == null)
                            TryJoinLobby();
                    }
                    else
                    {
                        authUI.ExistLoginInfo(false);
                        authUI.Init(authManager.User.Email, setting.isSaveID, setting.isAutoSave);
                        Debug.Log("signout");
                        GameManager.auth.SignOut();

                    }


                }
                else
                {
                    GameManager.auth.SignOut();
                    authUI.ExistLoginInfo(false);
                }
            }
            else
            {
                Debug.Log("guest");
                authUI.ExistLoginInfo(true);
                TryJoinLobby();
            }

            return;
        }
        else
        {
            if (LocalSaveLoader.LoadDataWithLocal("LoginSetting", out LoginSetting setting))
            {
                authUI.Init(setting.email, setting.isSaveID, setting.isAutoSave);
            }
            authUI.ExistLoginInfo(false);
        }

    }
    public void PressSettingButton()
    {

    }
    public void PressExitButton()
    {
        Application.Quit();
    }
    public async void TryJoinLobby()
    {
        authUI.SetupStateText("로비접속을 시도하고 있습니다.");
        Debug.LogWarning("networkmanagersutdown");
        StartGameResult result = await GameManager.network.JoinLobby();
        if (result.Ok)
        {
            Debug.Log("LobbyJoint Success");
            authUI.gameObject.SetActive(false);
            lobbyUI.gameObject.SetActive(true);
            return;
        }

        Debug.Log("LobbyJoint Failed");

        GameManager.UI.ShowPopUpUI(messageBoxUIPrefab).Init("로비 접속 실패", "로비 접속에 실패하였습니다.", null);

    }
}
