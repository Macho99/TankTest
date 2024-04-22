using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MessageBoxUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private RectTransform buttonRect;
    [SerializeField] private Button buttonPrefab;



    public void Init(string title, string content, params CreateButton[] createButton)
    {
        titleText.text = title;
        contentText.text = content;


        if (createButton != null)
        {
            for (int i = 0; i < createButton.Length; i++)
            {
                Button button = Instantiate(buttonPrefab, buttonRect);
                TextMeshProUGUI buttonName = button.GetComponentInChildren<TextMeshProUGUI>();
                buttonName.text = createButton[i].buttonName;
                Debug.Log(buttonName.text);
                button.onClick.AddListener(createButton[i].onClick);
                button.onClick.AddListener(() => { Destroy(gameObject); });
            }
        }
        else
        {
            Button button = Instantiate(buttonPrefab, buttonRect);
            TextMeshProUGUI buttonName = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonName.text = "È®ÀÎ";
            button.onClick.AddListener(() => { Destroy(gameObject); });
        }





        this.transform.SetAsLastSibling();
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
