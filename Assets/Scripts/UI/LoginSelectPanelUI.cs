using Firebase.Auth;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoginSelectPanelUI : PageUI
{
    [SerializeField] private CreateNicnameUI createNicnameUI;
    public UnityEvent onSetupEmailLogin;
    private void Awake()
    {


    }
    public void PressGuestLoginButton()
    {
        createNicnameUI.gameObject.SetActive(true);

    }
    public void PressEmailLoginButton()
    {    
        onSetupEmailLogin?.Invoke();
    }
}
