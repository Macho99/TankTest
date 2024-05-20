using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class LobbySessionUI : SimulationBehaviour
{
    [SerializeField] private RectTransform sessionItemListTr;
    [SerializeField] private SessionItem sessionItemPrefab;
    [SerializeField] private CreateSessionUI createSessionUI;
    [SerializeField] private SessionUI sessionUI;
    [SerializeField] private PrivateSessionConnetUI privateSessionConnetUI;
    public UnityEvent onConnetSession;

    private void Awake()
    {

    }
    private void OnEnable()
    {
       
        GameManager.network.onSessionUpdate += UpdateSession;
    }
    private void OnDisable()
    {

        GameManager.network.onSessionUpdate -= UpdateSession;

    }
    public void UpdateSession(List<SessionInfo> sessionInfo)
    {
        foreach (Transform prevItem in sessionItemListTr.transform)
        {
            Destroy(prevItem.gameObject);
        }

        foreach (SessionInfo session in sessionInfo)
        {

            SessionItem item = Instantiate(sessionItemPrefab, sessionItemListTr);

            Debug.Log(session.Properties.ContainsKey("Password"));
            item.CreateSessionItem(session, sessionUI.JoinSession, session.Properties.ContainsKey("Password") ? privateSessionConnetUI : null);

        }

    }
    public void PressCreateSessionButton()
    {

        createSessionUI.gameObject.SetActive(true);

    }
    public async void PressRandomJoinButton()
    {
        LoadingUI loadingUI = GameManager.UI.ShowPopUpUI<LoadingUI>("UI/LoadingUI");
        loadingUI.Init("���� ������ ���� �濡 ���� ���Դϴ�.");
        StartGameResult result = await GameManager.network.JoinRandomSession();

        if (result != null)
        {
            onConnetSession?.Invoke();
            loadingUI.CloseUI();
        }
        else
        {
            GameManager.UI.ShowPopUpUI<MessageBoxUI>("UI/MessageBoxUI").Init("�� ���� ����", "���� ������ ���� ���� �����ϴ�.", null);

            print("��");
            loadingUI.CloseUI();
        }



    }

}
