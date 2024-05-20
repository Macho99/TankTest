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
        AuthManager authManager = GameManager.auth;
        if (authManager.User != null)
        {
            playerNicnameText.text = authManager.Auth.CurrentUser.DisplayName;
        }
    }
    public async void PressLogOutButton()
    {
        AuthManager authManager = GameManager.auth;
        if (authManager.User.IsAnonymous)
        {
            authManager.DeletUser();
        }
        else
        {
            authManager.SignOut();
        }



        onLogout?.Invoke();
        await GameManager.network.ExitLobby(false);

        this.gameObject.SetActive(false);
    }
}
