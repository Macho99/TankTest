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
    private MessageBoxUI messageBoxUIPrefab;
    private LoadingUI loadingUIPrefab;
    public UnityEvent onSuccessRegist;
    private void OnDisable()
    {
        emailInputfield.text = string.Empty;
        pwInputfield.text = string.Empty;
        pwCheckInputfield.text = string.Empty;
        nicnameInputfield.text = string.Empty;
        signupButton.interactable = false;
        loadingUIPrefab = GameManager.Resource.Load<LoadingUI>("UI/LoadingUI");
        messageBoxUIPrefab = GameManager.Resource.Load<MessageBoxUI>("UI/MessageBoxUI");
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
            GameManager.UI.ShowPopUpUI(messageBoxUIPrefab).Init("회원가입 실패", "비밀번호가 서로 다릅니다.", null);
            return;
        }


        LoadingUI loadingUI = GameManager.UI.ShowPopUpUI(loadingUIPrefab);
        loadingUI.Init("회원 가입 시도중입니다.");

        AuthResult authResult = await GameManager.auth.SignUpWithEmailAndPassword(emailInputfield.text, pwInputfield.text, nicnameInputfield.text);
        if (authResult == null)
        {
            GameManager.UI.ShowPopUpUI(messageBoxUIPrefab).Init("회원가입 실패", "회원가입에 실패 하였습니다.", null);

        }
        else
        {
            onSuccessRegist?.Invoke();
        }

        loadingUI.CloseUI();
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
