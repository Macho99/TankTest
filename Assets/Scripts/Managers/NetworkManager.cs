using Fusion;
using Fusion.Addons.SimpleKCC;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using WebSocketSharp;


public enum SceneType { StartScene, RoomScene, LoadingScene, GameScene, Size }
[Serializable]
public class SceneData
{
    public SceneType SceneType;
    public string scenePath;
    public SceneRef GetScenRef()
    {
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);
        return SceneRef.FromIndex(sceneIndex);
    }
}
public class NetworkManager : MonoBehaviour
{

    [SerializeField] public SceneData[] sceneData;
    public enum PlaceType { None, Lobby, Session, Loading, Ingame }
    NetworkRunner runner;
    NetworkRunner lobbyRunner;

    private Dictionary<PlayerRef, NetworkObject> players = new Dictionary<PlayerRef, NetworkObject>();
    public event Action<List<SessionInfo>> onSessionUpdate;
    private List<SessionInfo> sessionInfos;
    private int numbering;
    private PlayerControls playerControls;

    public Action<NetworkRunner> onRunnerAction;

    public NetworkRunner Runner { get { return runner; } set { runner = value; } }


    public void SetPlayer(PlayerRef player, NetworkObject playerObject)
    {
        players.Add(player, playerObject);
    }
    public bool GetPlayer(PlayerRef player, out NetworkObject playerObject)
    {
        if (players.TryGetValue(player, out NetworkObject findPlayerObject))
        {
            playerObject = findPlayerObject;
            return true;
        }
        playerObject = null;
        return false;
    }
    public void RemovePlayer(PlayerRef player)
    {
        if (players.ContainsKey(player))
        {
            players.Remove(player);

        }
    }

    public SceneRef GetSceneRef(SceneType sceneType)
    {
        return sceneData[(int)sceneType].GetScenRef();
    }
    private void Awake()
    {
        numbering = 0;
    }
    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
        }
        playerControls.Enable();
    }
    private void OnDisable()
    {
        playerControls.Disable();
    }
    public async Task<StartGameResult> JoinLobby()
    {
        if (runner != null)
        {

            Debug.LogWarning("networkmanagersutdown");
            if (runner.IsSceneAuthority)
                await runner.UnloadScene(sceneData[(int)SceneType.RoomScene].GetScenRef());
            else
            {
                SceneManager.UnloadSceneAsync(sceneData[(int)SceneType.RoomScene].GetScenRef().AsIndex);

            }
            await runner.Shutdown(true);
            runner = null;
        }


        runner = GameManager.Resource.Instantiate<NetworkRunner>("Other/Runner");
        runner.gameObject.name = "SessionRunner" + numbering;

        numbering++;
        onRunnerAction?.Invoke(runner);
        NetworkEvents networkEvents = runner.GetComponent<NetworkEvents>();
        networkEvents.OnSessionListUpdate.AddListener(OnSessionListUpdated);


        Debug.LogWarning(runner.gameObject.name);
        StartGameResult result = await runner.JoinSessionLobby(SessionLobby.ClientServer);

        if (result.Ok)
        {
            Debug.Log("Lobby Join Success");
        }
        else
        {
            Debug.Log($"Lobby Join Failed : {result.ErrorMessage}");
        }
        return result;
    }
    public async Task<StartGameResult> CreateRoom(string sessionName, int maxCount, string password = null)
    {
        Dictionary<string, SessionProperty> sessionProperty = null;

        if (!password.IsNullOrEmpty())
        {
            sessionProperty = new Dictionary<string, SessionProperty>();
            sessionProperty["Password"] = password;
        }
        var sceneInfo = new NetworkSceneInfo();

        sceneInfo.AddSceneRef(sceneData[(int)SceneType.RoomScene].GetScenRef(), LoadSceneMode.Additive, activeOnLoad: true);
        INetworkSceneManager networkSceneManager = runner.GetComponent<LoadSceneManager>();

        StartGameArgs startGame = new StartGameArgs();
        startGame.SessionName = sessionName;
        startGame.GameMode = GameMode.Host;
        startGame.IsVisible = true;
        startGame.IsOpen = true;
        startGame.SessionProperties = sessionProperty;
        startGame.PlayerCount = maxCount;
        startGame.SceneManager = networkSceneManager;
        startGame.Scene = sceneInfo;


        StartGameResult result = await runner.StartGame(startGame);
      
        runner.ProvideInput = true;
        return result;
    }
    public async Task<StartGameResult> JoinRandomSession()
    {


        StartGameResult result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,

        });

        if (!result.Ok)
        {
            runner = GameManager.Resource.Instantiate<NetworkRunner>("Other/Runner");
            runner.gameObject.name = "SessionRunner" + numbering;
            numbering++;
            onRunnerAction?.Invoke(runner);
            NetworkEvents networkEvents = runner.GetComponent<NetworkEvents>();
            networkEvents.OnSessionListUpdate.AddListener(OnSessionListUpdated);


            Debug.LogWarning(runner.gameObject.name);
            await runner.JoinSessionLobby(SessionLobby.ClientServer);
        }

        return result;


    }

    public async Task<StartGameResult> JoinSession(SessionInfo sessionInfo, string password = null)
    {
        Dictionary<string, SessionProperty> sessionProperty = null;

        if (password != null)
        {
            sessionProperty = new Dictionary<string, SessionProperty>();
            sessionProperty["Password"] = password;
        }
        runner.ProvideInput = true;
        StartGameResult result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionInfo.Name,
            SessionProperties = sessionProperty,
            IsVisible = sessionInfo.IsVisible,

        });
        return result;
    }
    public async Task ExitLobby(bool isLobby)
    {
        if (runner != null)
        {
            onRunnerAction?.Invoke(null);
            await runner.Shutdown();
            runner = null;
            if (isLobby == true)
            {
                runner = GameManager.Resource.Instantiate<NetworkRunner>("Other/Runner");
                onRunnerAction?.Invoke(runner);
                StartGameResult result = await runner.JoinSessionLobby(SessionLobby.ClientServer);


            }
        }
    }
    public async void JoinIngame()
    {

        LoadSceneManager loadSceneManager = runner.GetComponent<LoadSceneManager>();
        await loadSceneManager.LoadGameScene();


    }

    void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        sessionInfos = sessionList;
        onSessionUpdate?.Invoke(sessionList);
    }


}
public enum ButtonType
{ PlayerMove, MouseDelta, Run, Jump, Crouch, MouseLock, Interact, Reload, Adherence, ActiveItemContainer, PutWeapon, FirstWeapon, SecondWeapon, SubWeapon, MilyWeapon, BombWeapon, Attack,ZombieInfoOpen ,Size }
public struct NetworkInputData : INetworkInput
{

    public NetworkButtons buttons;
    public Vector2 inputDirection;
    public Vector2 mouseDelta;

}