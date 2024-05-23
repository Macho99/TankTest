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
    [SerializeField] private AudioClip buttonClickSFX;
    public UnityEvent onSetupEmailLogin;
    private void Awake()
    {


    }
    public void PressGuestLoginButton()
    {
        GameManager.Sound.PlayOneShot(buttonClickSFX, AudioGroup.SFX, Camera.main.transform, false);
        createNicnameUI.gameObject.SetActive(true);

    }
    public void PressEmailLoginButton()
    {
        GameManager.Sound.PlayOneShot(buttonClickSFX, AudioGroup.SFX, Camera.main.transform, false);
        onSetupEmailLogin?.Invoke();
    }
}
