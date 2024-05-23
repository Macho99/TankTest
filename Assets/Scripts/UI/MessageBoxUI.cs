using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MessageBoxUI : PopUpUI
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private RectTransform buttonRect;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private AudioClip buttonClickSFX;
    private List<Button> buttonList = new List<Button>();


    public void Init(string title, string content, params CreateButton[] createButton)
    {
        titleText.text = title;
        contentText.text = content;


        if (createButton != null)
        {
            for (int i = 0; i < createButton.Length; i++)
            {
                Button button = Instantiate(buttonPrefab, buttonRect);
                buttonList.Add(button);
                TextMeshProUGUI buttonName = button.GetComponentInChildren<TextMeshProUGUI>();
                buttonName.text = createButton[i].buttonName;
                Debug.Log(buttonName.text);
                button.onClick.AddListener(createButton[i].onClick);
                button.onClick.AddListener(() => { CloseUI(); });
            }
        }
        else
        {
            Button button = Instantiate(buttonPrefab, buttonRect);
            buttonList.Add(button);
            TextMeshProUGUI buttonName = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonName.text = "È®ÀÎ";
            button.onClick.AddListener(() => { CloseUI(); });
        }

        this.transform.SetAsLastSibling();
        Debug.LogWarning("true");
        gameObject.SetActive(true);

    }
    public override void CloseUI()
    {
        GameManager.Sound.PlayOneShot(buttonClickSFX, AudioGroup.SFX, Camera.main.transform, false);
        for (int i = 0; i < buttonList.Count; i++)
        {
            Destroy(buttonList[i].gameObject);
            buttonList.Remove(buttonList[i]);
        }
        base.CloseUI();
    }
}
public class CreateButton
{
    public string buttonName;
    public UnityAction onClick;

    public CreateButton(string buttonName, UnityAction onClick)
    {
        this.buttonName = buttonName;
        this.onClick = onClick;
    }
}
