using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SessionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sessionNameTMP;
    [SerializeField] private TextMeshProUGUI sessionCountTMP;
    [SerializeField] private RectTransform sessionUserList;
    [SerializeField] private SessionUserUI sessionUserItemPrefab;
    [SerializeField] private TextMeshProUGUI readyOrStartText;
    [SerializeField] private AudioClip buttonClickSFX;
    private NetworkManager networkManager;
    private SessionInfo sessionInfo;
    private LoadingUI loadingUIPrefab;

    private Dictionary<PlayerRef, RoomPlayer> sessionUserDic;
    private void Awake()
    {
        sessionUserDic = new Dictionary<PlayerRef, RoomPlayer>();
        loadingUIPrefab = GameManager.Resource.Load<LoadingUI>("UI/LoadingUI");
        LoadingUI loadingUI = GameManager.UI.ShowPopUpUI(loadingUIPrefab);
        loadingUI.CloseUI();
    }
    private void OnEnable()
    {
        if (networkManager == null)
            networkManager = GameManager.network;

        sessionInfo = networkManager.Runner.SessionInfo;
        sessionNameTMP.text = sessionInfo.Name;
        sessionCountTMP.text = $"({sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers})";
    }
   

    private void Update()
    {
        if (networkManager == null)
            return;

        if (networkManager.Runner == null)
            return;

        foreach (var player in networkManager.Runner.ActivePlayers)
        {
            if (networkManager.Runner.TryGetPlayerObject(player, out var playerObject))
            {
                if (!sessionUserDic.ContainsKey(player))
                {
                    if (playerObject.TryGetComponent(out RoomPlayer roomPlayer))
                    {
                        SessionUserUI userUI = Instantiate(sessionUserItemPrefab, sessionUserList);
                        roomPlayer.Setup(userUI, player);
                        sessionUserDic.Add(player, roomPlayer);
                        roomPlayer.onDespawn += () => { sessionUserDic.Remove(player); };

                        if (networkManager.Runner.TryGetPlayerObject(networkManager.Runner.LocalPlayer, out NetworkObject localPlayer))
                        {
                            if (localPlayer.GetComponent<RoomPlayer>().isHost)
                            {
                                readyOrStartText.text = "시작";
                            }
                            else
                            {
                                readyOrStartText.text = "준비";

                            }
                        }
                    }
                }
            }
        }
        sessionCountTMP.text = $"({sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers})";
    }

    public void PressButton()
    {
        GameManager.Sound.PlayOneShot(buttonClickSFX, AudioGroup.SFX, Camera.main.transform, false);
    }
    public async void ExitSession()
    {
        LoadingUI loadingUI = GameManager.UI.ShowPopUpUI(loadingUIPrefab);
        loadingUI.Init("방을 나가는 중입니다.");
        Debug.LogWarning("networkmanagersutdown");
        await networkManager.JoinLobby();

        loadingUI.CloseUI();
    }
    public void PressReadyOrStartButton()
    {
        PlayerRef playerRef = networkManager.Runner.LocalPlayer;
        if (sessionUserDic.ContainsKey(playerRef))
        {
            if (!sessionUserDic[playerRef].isHost)
            {
                PlayerPreviewController playerPreview = FindObjectOfType<PlayerPreviewController>();
                sessionUserDic[playerRef].RPC_Ready();
                sessionUserDic[playerRef].RPC_AddClientPreset(AppearanceType.Preset, playerPreview.GetCurrenIndex(AppearanceType.Preset));
                sessionUserDic[playerRef].RPC_AddClientPreset(AppearanceType.Color, playerPreview.GetCurrenIndex(AppearanceType.Color));
                sessionUserDic[playerRef].RPC_AddClientPreset(AppearanceType.Hair, playerPreview.GetCurrenIndex(AppearanceType.Hair));
                sessionUserDic[playerRef].RPC_AddClientPreset(AppearanceType.Breard, playerPreview.GetCurrenIndex(AppearanceType.Breard));
            }
            else
            {
                int readyCount = 1;
                foreach (var player in sessionUserDic.Values)
                {
                    readyCount += player.IsReady ? 1 : 0;
                }

                if (readyCount == sessionUserDic.Count)
                    sessionUserDic[playerRef].StartGame();
            }
        }
    }
    public void PressExitGame()
    {
        Application.Quit();
    }
}
