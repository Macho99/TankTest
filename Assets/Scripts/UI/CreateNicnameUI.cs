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


        GameManagers.Instance.UIManager.ActiveLoading(true, "ȸ�������� �õ� �ϰ��ֽ��ϴ�.");
        AuthResult result = await GameManagers.Instance.AuthManager.LoginWithGuest();
        if (result == null)
        {
            return;
        }
        Debug.Log(nicnameInputField.text);
        GameManagers.Instance.UIManager.ActiveLoading(true, "�г����� ���� �ϰ��ֽ��ϴ�.");
        await GameManagers.Instance.AuthManager.UpdateUserProfile(nicnameInputField.text);

        LoginSetting setting = new LoginSetting(null, true, true);
        LocalSaveLoader.SaveDataWithLocal("LoginSetting", setting);
        GameManagers.Instance.UIManager.ActiveLoading(true, "�κ� ������ �õ��ϰ��ֽ��ϴ�.");
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
