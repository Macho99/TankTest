using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public enum PlaceType { None, Lobby, Session, Loading, Ingame }
    [SerializeField] private NetworkRunner runnerPrefab;

    [SerializeField] private NetworkObject roomPlayerPrefab;
    NetworkRunner runner;

    public event Action<List<SessionInfo>> onSessionUpdate;

    private PlaceType placeType;
    private int numbering;
    private PlayerControls playerControls;
    public PlaceType CurrentPlace { get { return placeType; } set { placeType = value; } }
    private Dictionary<PlayerRef, NetworkObject> players = new Dictionary<PlayerRef, NetworkObject>();

    public NetworkRunner Runner { get { return runner; } }
    private List<SessionInfo> sessionInfos;
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
    private void nDisable()
    {
        playerControls.Disable();
    }
    public async Task<StartGameResult> JoinLobby()
    {
        if (runner != null)
        {
            await runner.Shutdown(true);
            runner = null;
        }

        runner = Instantiate(runnerPrefab);
        runner.gameObject.name = "SessionRunner" + numbering;
        numbering++;

        runner.AddCallbacks(this);
        LoadSceneManager networkSceneManager = runner.GetComponent<LoadSceneManager>();
        networkSceneManager.Init(this);
        StartGameResult result = await runner.JoinSessionLobby(SessionLobby.ClientServer);

        if (result.Ok)
        {
            Debug.Log("Lobby Join Success");
            placeType = PlaceType.Lobby;
        }
        else
        {
            Debug.Log($"Lobby Join Failed : {result.ErrorMessage}");
        }
        return result;
    }
    public async Task<StartGameResult> CreateSession(string sessionName, int maxCount, string password = null)
    {
        Dictionary<string, SessionProperty> sessionProperty = null;

        if (password != null)
        {
            sessionProperty = new Dictionary<string, SessionProperty>();
            sessionProperty["Password"] = password;
        }

        runner.ProvideInput = true;

        INetworkSceneManager networkSceneManager = runner.GetComponent<LoadSceneManager>();
        StartGameResult result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = sessionName,
            IsVisible = true,
            IsOpen = true,
            SessionProperties = sessionProperty,
            PlayerCount = maxCount,
            EnableClientSessionCreation = true,
            SceneManager = networkSceneManager
        });
        if (!result.Ok)
        {
            Debug.Log(result.ErrorMessage);

        }
        else
        {
            placeType = PlaceType.Session;

        }

        return result;
    }
    public async Task<StartGameResult> JoinRandomSession()
    {
        try
        {
            if (sessionInfos == null || sessionInfos.Count == 0)
            {
                placeType = PlaceType.Lobby;
                return null;
            }

            StartGameResult result = await runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Client,

            });
            if (result.Ok)
            {
                placeType = PlaceType.Session;
            }
            else
            {
                placeType = PlaceType.Lobby;

            }
            return result;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }





    }
    public void RemovePlayer(PlayerRef player)
    {
        if (players.TryGetValue(player, out NetworkObject playerObject))
        {
            players.Remove(player);
        }
    }
    public void AddPlayer(PlayerRef player)
    {

    }
    public async Task<StartGameResult> JoinSession(SessionInfo sessionInfo, string password = null)
    {
        Dictionary<string, SessionProperty> sessionProperty = null;

        if (password != null)
        {
            sessionProperty = new Dictionary<string, SessionProperty>();
            sessionProperty["Password"] = password;
        }

        StartGameResult result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = sessionInfo.Name,
            SessionProperties = sessionProperty,
            IsVisible = sessionInfo.IsVisible,

        });
        if (result.Ok)
        {
            placeType = PlaceType.Session;
        }
        return result;
    }
    public async Task ExitLobby(bool isLobby)
    {
        if (runner != null)
        {
            await runner.Shutdown();
            runner = null;
            if (isLobby == true)
            {
                runner = Instantiate(runnerPrefab);
                StartGameResult result = await runner.JoinSessionLobby(SessionLobby.ClientServer);
                if (result.Ok)
                {
                    placeType = PlaceType.Lobby;
                }

            }
        }
    }
    public async void JoinIngame()
    {


        SceneRef sceneRef = SceneRef.FromIndex(1);

        LoadSceneManager loadSceneManager = runner.GetComponent<LoadSceneManager>();
        Debug.Log("state");
        await loadSceneManager.LoadGameScene(sceneRef);



        //runner.LoadScene(sceneRef,)
        //  await runner.UnloadScene(SceneRef.FromIndex(0));

    }
    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData data = new NetworkInputData();
        data.inputDirection = playerControls.Player.Move.ReadValue<Vector2>();

        input.Set(data);

    }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            if (placeType == PlaceType.Session)
            {
                NetworkObject roomPlayer = runner.Spawn(roomPlayerPrefab, Vector3.zero, Quaternion.identity, player);
                players[player] = roomPlayer;
                runner.SetPlayerObject(player, roomPlayer);
            }
        }
        Debug.Log("join");
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            if (runner.TryGetPlayerObject(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
            }
        }

    }
    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        sessionInfos = sessionList;
        onSessionUpdate?.Invoke(sessionList);
    }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("OnConnectedToServer");
    }
    #region ServerEvent
    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }


    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }



    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
    {

    }

    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
    {

    }

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }
    #endregion
}
public struct NetworkInputData : INetworkInput
{
    public enum ButtonType
    { Run = 0, Jump = 1, Crouch = 2, MouseLock = 3, Interact = 4 ,DebugText = 5 }
    public NetworkButtons buttons;
    public Vector2 inputDirection;
    public Vector2 mouseDelta;

}