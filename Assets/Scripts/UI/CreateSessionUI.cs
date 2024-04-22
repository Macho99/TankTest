using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CreateSessionUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private Slider maxpPlayerCountSlider;
    [SerializeField] private TextMeshProUGUI maxPlayerCoutnTMP;
    [SerializeField] private Toggle publicToggle;
    [SerializeField] private Toggle privateToggle;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private SessionUI sessionUI;

    public UnityEvent onCreateSession;

    private void OnEnable()
    {
        nameField.text = string.Empty;
        publicToggle.isOn = true;
        passwordField.interactable = false;
        passwordField.text = string.Empty;
        maxpPlayerCountSlider.value = 5;
        maxPlayerCoutnTMP.text = maxpPlayerCountSlider.value.ToString();
    }
    public void SlideCountChange(float value)
    {
        maxPlayerCoutnTMP.text = value.ToString();
    }
    public async void PressCreateSessionButton()
    {
        GameManagers.Instance.UIManager.ActiveLoading(true, "방을 만드는 중입니다.");
        Debug.Log(nameField.text);
        StartGameResult result = await GameManagers.Instance.NetworkManager.CreateSession(nameField.text, Convert.ToInt32(maxPlayerCoutnTMP.text), passwordField.text);
        if (result.Ok)
        {
            Debug.Log("createSession");

            onCreateSession?.Invoke();
            sessionUI.CreateSession(nameField.text, (int)maxpPlayerCountSlider.value);
            gameObject.SetActive(false);
        }
        GameManagers.Instance.UIManager.ActiveLoading(false);
    }
    public void PressCancelButton()
    {
        gameObject.SetActive(false);
    }



}
