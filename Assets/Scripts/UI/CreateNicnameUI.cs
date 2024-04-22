using Firebase.Auth;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CreateNicnameUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nicnameInputField;

    public UnityEvent onSuccess;

    private void OnDisable()
    {
        nicnameInputField.text = string.Empty;
    }
    public async void PressRegistButton()
    {
        if (nicnameInputField.text == string.Empty)
            return;


        GameManagers.Instance.UIManager.ActiveLoading(true, "회원가입을 시도 하고있습니다.");
        AuthResult result = await GameManagers.Instance.AuthManager.LoginWithGuest();
        if (result == null)
        {
            return;
        }
        Debug.Log(nicnameInputField.text);
        GameManagers.Instance.UIManager.ActiveLoading(true, "닉네임을 설정 하고있습니다.");
        await GameManagers.Instance.AuthManager.UpdateUserProfile(nicnameInputField.text);

        LoginSetting setting = new LoginSetting(null, true, true);
        LocalSaveLoader.SaveDataWithLocal("LoginSetting", setting);
        GameManagers.Instance.UIManager.ActiveLoading(true, "로비에 접속을 시도하고있습니다.");
        StartGameResult joinLobbyResult = await GameManagers.Instance.NetworkManager.JoinLobby();
        if (joinLobbyResult.Ok)
        {
            this.gameObject.SetActive(false);
            onSuccess?.Invoke();
        }
        else
        {

        }
        GameManagers.Instance.UIManager.ActiveLoading(false, null);

    }
    public void PressCancelButton()
    {
        gameObject.SetActive(false);
    }


}
