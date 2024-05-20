using Fusion;
using Fusion.Addons.SimpleKCC;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks, IBeforeUpdate
{

    public enum PlaceType { None, Lobby, Session, Loading, Ingame }
    NetworkRunner runner;

    public event Action<List<SessionInfo>> onSessionUpdate;

    private PlaceType placeType;
    private int numbering;
    private PlayerControls playerControls;
    public PlaceType CurrentPlace { get { return placeType; } set { placeType = value; } }
    private Dictionary<PlayerRef, NetworkObject> players = new Dictionary<PlayerRef, NetworkObject>();

    public NetworkRunner Runner { get { return runner; } }
    private List<SessionInfo> sessionInfos;

    NetworkInputData playerInput = new NetworkInputData();
    Vector2Accumulator lookAccum = new Vector2Accumulator(0.02f, true);
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
            await runner.Shutdown(true);
            runner = null;
        }


        runner = GameManager.Resource.Instantiate<NetworkRunner>("Other/StartRunner");
        runner.gameObject.name = "SessionRunner" + numbering;
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

        if (password != string.Empty)
        {
            sessionProperty = new Dictionary<string, SessionProperty>();
            sessionProperty["Password"] = password;
            Debug.Log("»ý¼º");
        }
        runner.ProvideInput = true;
        runner.AddCallbacks(this);
        INetworkSceneManager networkSceneManager = runner.GetComponent<LoadSceneManager>();

        StartGameArgs startGame = new StartGameArgs();
        startGame.SessionName = sessionName;
        startGame.GameMode = GameMode.Host;
        startGame.IsVisible = true;
        startGame.IsOpen = true;
        startGame.SessionProperties = sessionProperty;
        startGame.PlayerCount = maxCount;
        startGame.SceneManager = networkSceneManager;


        StartGameResult result = await runner.StartGame(startGame);
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
                print("null");
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
            print(result);
            return result;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }



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
                runner = GameManager.Resource.Instantiate<NetworkRunner>("Other/StartRunner");
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
      

        int index = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/GameScene.unity");
        SceneRef sceneRef = SceneRef.FromIndex(index);

        LoadSceneManager loadSceneManager = runner.GetComponent<LoadSceneManager>();
        await loadSceneManager.LoadGameScene(sceneRef);


    }
    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        playerInput.inputDirection = playerControls.Player.Move.ReadValue<Vector2>();
        playerInput.buttons.Set(ButtonType.Run, playerControls.Player.Run.IsPressed());
        playerInput.buttons.Set(ButtonType.Jump, playerControls.Player.Jump.IsPressed());
        playerInput.buttons.Set(ButtonType.Crouch, playerControls.Player.Crouch.IsPressed());
        playerInput.buttons.Set(ButtonType.Interact, playerControls.Player.Interact.IsPressed());
        playerInput.buttons.Set(ButtonType.MouseLock, playerControls.Player.TestMouseCursurLock.IsPressed());
        playerInput.buttons.Set(ButtonType.Adherence, playerControls.Player.Adherence.IsPressed());
        playerInput.buttons.Set(ButtonType.ActiveItemContainer, playerControls.Player.ActiveItemContainer.IsPressed());
        playerInput.buttons.Set(ButtonType.PutWeapon, playerControls.Player.PutWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.FirstWeapon, playerControls.Player.FirstWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.SecondWeapon, playerControls.Player.SecondWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.SubWeapon, playerControls.Player.SubWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.MilyWeapon, playerControls.Player.MilyWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.BombWeapon, playerControls.Player.BombWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.Attack, playerControls.Player.Attack.IsPressed());
        playerInput.buttons.Set(ButtonType.Reload, playerControls.Player.Reload.IsPressed());
        playerInput.mouseDelta = Mouse.current.delta.ReadValue();
        //lookAccum.ConsumeTickAligned(runner);

        // Debug.Log(lookAccum.ConsumeTickAligned(runner));
        input.Set(playerInput);
        playerInput = default;

    }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {

            NetworkObject roomPlayerPrefab = GameManager.Resource.Load<NetworkObject>("Player/RoomPlayer");
            NetworkObject roomPlayer = runner.Spawn(roomPlayerPrefab, Vector3.zero, Quaternion.identity, player);
            players[player] = roomPlayer;
            runner.SetPlayerObject(player, roomPlayer);
            runner.MakeDontDestroyOnLoad(roomPlayer.gameObject);


        }
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        if (players.TryGetValue(player, out NetworkObject networkObject))
        {
            Debug.Log(player);

            runner.Despawn(networkObject);
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

    public void BeforeUpdate()
    {
        Debug.Log("asdasd");
        lookAccum.Accumulate(Mouse.current.delta.ReadValue());
    }
    #endregion
}
public enum ButtonType
{ PlayerMove, MouseDelta, Run, Jump, Crouch, MouseLock, Interact, Reload, Adherence, ActiveItemContainer, PutWeapon, FirstWeapon, SecondWeapon, SubWeapon, MilyWeapon, BombWeapon, Attack, Size }
public struct NetworkInputData : INetworkInput
{

    public NetworkButtons buttons;
    public Vector2 inputDirection;
    public Vector2 mouseDelta;

}