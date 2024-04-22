using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class EmailSignupUI : PageUI
{

    [SerializeField] private TMP_InputField emailInputfield;
    [SerializeField] private TMP_InputField pwInputfield;
    [SerializeField] private TMP_InputField pwCheckInputfield;
    [SerializeField] private TMP_InputField nicnameInputfield;
    [SerializeField] private Button signupButton;

    public UnityEvent onSuccessRegist;
    private void OnDisable()
    {
        emailInputfield.text = string.Empty;
        pwInputfield.text = string.Empty;
        pwCheckInputfield.text = string.Empty;
        nicnameInputfield.text = string.Empty;
        signupButton.interactable = false;

    }
    private void Update()
    {
        if (emailInputfield.text != string.Empty && pwInputfield.text != string.Empty && pwCheckInputfield.text != string.Empty && nicnameInputfield.text != string.Empty)
        {
            signupButton.interactable = true;
        }
        else
        {
            signupButton.interactable = false;
        }
    }

    public async void PressSignupButton()
    {
        if (pwInputfield.text != pwCheckInputfield.text)
        {
            GameManagers.Instance.UIManager.CreateMessageBoxUI("회원가입 실패", "비밀번호가 서로 다릅니다.", null);
            return;
        }


        GameManagers.Instance.UIManager.ActiveLoading(true, "회원 가입 시도중입니다.");

        AuthResult authResult = await GameManagers.Instance.AuthManager.SignUpWithEmailAndPassword(emailInputfield.text, pwInputfield.text, nicnameInputfield.text);
        if(authResult == null)
        {
            GameManagers.Instance.UIManager.CreateMessageBoxUI("회원가입 실패", "회원가입에 실패 하였습니다.", null);
           
        }
        else
        {
            onSuccessRegist?.Invoke();
        }

        GameManagers.Instance.UIManager.ActiveLoading(false, null);
    }
    public void ReadLogineResult(Task<AuthResult> result)
    {

        if (result.IsCanceled || result.IsFaulted)
        {
            Debug.Log(result.Exception);
        }
        else
        {
            Debug.Log("complete");
        }

    }
}
