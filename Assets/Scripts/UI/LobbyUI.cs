using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNicnameText;
    public UnityEvent onLogout;

    private void OnEnable()
    {
        AuthManager authManager = GameManagers.Instance.AuthManager;
        if (authManager.User != null)
        {
            playerNicnameText.text = authManager.Auth.CurrentUser.DisplayName;
        }
    }
    public async void PressLogOutButton()
    {
        AuthManager auth = GameManagers.Instance.AuthManager;
        if (auth.User.IsAnonymous)
        {
            GameManagers.Instance.AuthManager.DeletUser();
        }
        else
        {
            GameManagers.Instance.AuthManager.SignOut();
        }



        onLogout?.Invoke();
        await GameManagers.Instance.NetworkManager.ExitLobby(false);

        this.gameObject.SetActive(false);
    }
}
