using Firebase.Auth;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AuthUI : MonoBehaviour
{
    public enum AuthPageUIType : int { LoginSelect, Login, Signup }
    [SerializeField] private PageUI[] authPageUI;
    [SerializeField] private TextMeshProUGUI stateInfoText;

    private AuthPageUIType currentPage;
    private void Awake()
    {
        currentPage = AuthPageUIType.LoginSelect;
    }
    public void Init(string email, bool isSaveID, bool IsAutoLogin)
    {
        ((EmailLoginUI)authPageUI[(int)AuthPageUIType.Login]).Init(email, isSaveID, IsAutoLogin);
    }


    public void SetupStateText(string stateText)
    {
        stateInfoText.text = stateText;
    }
    public void ActivePageUI(int newtype)
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        authPageUI[(int)currentPage].gameObject.SetActive(false);

        currentPage = (AuthPageUIType)newtype;

        authPageUI[(int)currentPage].gameObject.SetActive(true);

    }
    public void ExistLoginInfo(bool isExist)
    {

        stateInfoText.gameObject.SetActive(isExist);

        if (isExist == false)
        {
            ActivePageUI((int)AuthPageUIType.LoginSelect);
        }
    }

    public void PressGuestLogin()
    {

    }
    public void PressEmailLogin()
    {
        gameObject.SetActive(false);

    }



}
