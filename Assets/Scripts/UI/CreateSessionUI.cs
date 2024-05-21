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
        LoadingUI loadingUI = GameManager.UI.ShowPopUpUI<LoadingUI>("UI/LoadingUI");
        loadingUI.Init("���� ����� ���Դϴ�.");
      
        StartGameResult result = await GameManager.network.CreateRoom(nameField.text, Convert.ToInt32(maxPlayerCoutnTMP.text), passwordField.text);
        if (result.Ok)
        {
            onCreateSession?.Invoke();
            gameObject.SetActive(false);
        }
        loadingUI.CloseUI();
    }
    public void PressCancelButton()
    {
        gameObject.SetActive(false);
    }



}
