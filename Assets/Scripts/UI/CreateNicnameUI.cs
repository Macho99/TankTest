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
    [SerializeField] private AudioClip buttonClickSFX;
    public UnityEvent onSuccess;

    private void OnDisable()
    {
        nicnameInputField.text = string.Empty;
    }
    public async void PressRegistButton()
    {
        GameManager.Sound.PlayOneShot(buttonClickSFX, AudioGroup.SFX, Camera.main.transform, false);
        if (nicnameInputField.text == string.Empty)
            return;
       
        LoadingUI loadingUI = GameManager.UI.ShowPopUpUI<LoadingUI>("UI/LoadingUI");
        loadingUI.Init("ȸ�������� �õ� �ϰ��ֽ��ϴ�.");

        AuthResult result = await GameManager.auth.LoginWithGuest();
        if (result == null)
        {
            print("null");
            return;
        }
        loadingUI.Init("�г����� ���� �ϰ��ֽ��ϴ�.");
        await GameManager.auth.UpdateUserProfile(nicnameInputField.text);

        LoginSetting setting = new LoginSetting(null, true, true);
        LocalSaveLoader.SaveDataWithLocal("LoginSetting", setting);
        loadingUI.Init("�κ� ������ �õ��ϰ��ֽ��ϴ�.");
        Debug.LogWarning("networkmanagersutdown");
        StartGameResult joinLobbyResult = await GameManager.network.JoinLobby();
        if (joinLobbyResult.Ok)
        {
            this.gameObject.SetActive(false);
            onSuccess?.Invoke();
        }
        else
        {

        }
        loadingUI.CloseUI();

    }
    public void PressCancelButton()
    {
        GameManager.Sound.PlayOneShot(buttonClickSFX, AudioGroup.SFX, Camera.main.transform, false);
        gameObject.SetActive(false);
    }


}
