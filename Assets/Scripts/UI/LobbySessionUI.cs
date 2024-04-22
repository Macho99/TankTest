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

        GameManagers.Instance.NetworkManager.onSessionUpdate += UpdateSession;
    }
    private void OnDisable()
    {

        GameManagers.Instance.NetworkManager.onSessionUpdate -= UpdateSession;

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
            item.CreateSessionItem(session, sessionUI.JoinSession, session.Properties["Password"] != null ? privateSessionConnetUI : null);
        }

    }
    public void PressCreateSessionButton()
    {

        createSessionUI.gameObject.SetActive(true);

    }
    public async void PressRandomJoinButton()
    {
        GameManagers.Instance.UIManager.ActiveLoading(true, "접속 가능한 게임 방에 접속 중입니다.");
        StartGameResult result = await GameManagers.Instance.NetworkManager.JoinRandomSession();

        if (result != null)
        {
            onConnetSession?.Invoke();
        }
        else
        {
            GameManagers.Instance.UIManager.CreateMessageBoxUI("방 접속 실패", "접속 가능한 게임 방이 없습니다.", null);
        }

        GameManagers.Instance.UIManager.ActiveLoading(false);

    }

}
