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

public class NetworkManager : MonoBehaviour
{

    public enum PlaceType { None, Lobby, Session, Loading, Ingame }
    NetworkRunner runner;
    NetworkRunner lobbyRunner;

    public event Action<List<SessionInfo>> onSessionUpdate;
    private List<SessionInfo> sessionInfos;
    private PlaceType placeType;
    private int numbering;
    private PlayerControls playerControls;

    public Action<NetworkRunner> onRunnerAction;
    public PlaceType CurrentPlace { get { return placeType; } set { placeType = value; } }

    public NetworkRunner Runner { get { return runner; } }


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
            onRunnerAction?.Invoke(null);
            await runner.Shutdown(true);
            runner = null;
        }


        runner = GameManager.Resource.Instantiate<NetworkRunner>("Other/Runner");
        runner.gameObject.name = "SessionRunner" + numbering;
        NetworkEvents networkEvents = runner.GetComponent<NetworkEvents>();
        networkEvents.OnSessionListUpdate.AddListener(OnSessionListUpdated);
        onRunnerAction?.Invoke(runner);

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
        runner.ProvideInput = true;
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
            onRunnerAction?.Invoke(null);
            await runner.Shutdown();
            runner = null;
            if (isLobby == true)
            {
                runner = GameManager.Resource.Instantiate<NetworkRunner>("Other/Runner");
                onRunnerAction?.Invoke(runner);
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
        int index = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/Test/TestScene.unity");
        SceneRef sceneRef = SceneRef.FromIndex(index);

        LoadSceneManager loadSceneManager = runner.GetComponent<LoadSceneManager>();
        await loadSceneManager.LoadGameScene(sceneRef);


    }
   
    void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        sessionInfos = sessionList;
        onSessionUpdate?.Invoke(sessionList);
    }

  
}
public enum ButtonType
{ PlayerMove, MouseDelta, Run, Jump, Crouch, MouseLock, Interact, Reload, Adherence, ActiveItemContainer, PutWeapon, FirstWeapon, SecondWeapon, SubWeapon, MilyWeapon, BombWeapon, Attack, Size }
public struct NetworkInputData : INetworkInput
{

    public NetworkButtons buttons;
    public Vector2 inputDirection;
    public Vector2 mouseDelta;

}