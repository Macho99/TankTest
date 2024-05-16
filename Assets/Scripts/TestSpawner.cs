using Fusion;
using Fusion.Addons.SimpleKCC;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TestSpawner : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef testPlayer;
    NetworkRunner runner;
    private PlayerControls playerControls;
    [SerializeField] private Transform spawnPoint;
    private Dictionary<PlayerRef, NetworkObject> playerObjects = new Dictionary<PlayerRef, NetworkObject>();

    NetworkInputData playerInput = new NetworkInputData();
    Vector2Accumulator lookAccum = new Vector2Accumulator(0.02f, true);
    public void BeforeUpdate()
    {
        playerInput.inputDirection = playerControls.Player.Move.ReadValue<Vector2>();
        lookAccum.Accumulate(Mouse.current.delta.ReadValue());
        //playerInput.mouseDelta = Mouse.current.delta.ReadValue();
       
    }

    private async void Awake()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.Enable();

        }


        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();

        }
        runner.ProvideInput = true;
        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            Scene = scene,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
            ObjectProvider = runner.GetComponent<INetworkObjectProvider>()

        });
        runner.AddCallbacks(this);
    }
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
    {

    }

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

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        playerInput.buttons.Set(ButtonType.Run, playerControls.Player.Run.IsPressed());
        playerInput.buttons.Set(ButtonType.Jump, playerControls.Player.Jump.IsPressed());
        playerInput.buttons.Set(ButtonType.Crouch, playerControls.Player.Crouch.IsPressed());
        playerInput.buttons.Set(ButtonType.Interact, playerControls.Player.Interact.IsPressed());
        playerInput.buttons.Set(ButtonType.MouseLock, playerControls.Player.TestMouseCursurLock.IsPressed());
        playerInput.buttons.Set(ButtonType.DebugText, playerControls.Player.DebugText.IsPressed());
        playerInput.buttons.Set(ButtonType.Adherence, playerControls.Player.Adherence.IsPressed());
        playerInput.buttons.Set(ButtonType.ActiveItemContainer, playerControls.Player.ActiveItemContainer.IsPressed());
        playerInput.buttons.Set(ButtonType.PutWeapon, playerControls.Player.PutWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.FirstWeapon, playerControls.Player.FirstWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.SecondWeapon, playerControls.Player.SecondWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.SubWeapon, playerControls.Player.SubWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.MilyWeapon, playerControls.Player.MilyWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.BombWeapon, playerControls.Player.BombWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.Attack, playerControls.Player.Attack.IsPressed());
        playerInput.mouseDelta = lookAccum.ConsumeTickAligned(runner);
        //NetworkInputData data = new NetworkInputData();
        //data.inputDirection = playerControls.Player.Move.ReadValue<Vector2>();
        //data.mouseDelta = Mouse.current.delta.ReadValue();
        //data.buttons.Set(ButtonType.Run, playerControls.Player.Run.IsPressed());
        //data.buttons.Set(ButtonType.Jump, playerControls.Player.Jump.IsPressed());
        //data.buttons.Set(ButtonType.Crouch, playerControls.Player.Crouch.IsPressed());
        //data.buttons.Set(ButtonType.Interact, playerControls.Player.Interact.IsPressed());
        //data.buttons.Set(ButtonType.MouseLock, playerControls.Player.TestMouseCursurLock.IsPressed());
        //data.buttons.Set(ButtonType.DebugText, playerControls.Player.DebugText.IsPressed());
        //data.buttons.Set(ButtonType.Adherence, playerControls.Player.Adherence.IsPressed());
        //data.buttons.Set(ButtonType.ActiveItemContainer, playerControls.Player.ActiveItemContainer.IsPressed());
        //data.buttons.Set(ButtonType.PutWeapon, playerControls.Player.PutWeapon.IsPressed());
        //data.buttons.Set(ButtonType.FirstWeapon, playerControls.Player.FirstWeapon.IsPressed());
        //data.buttons.Set(ButtonType.SecondWeapon, playerControls.Player.SecondWeapon.IsPressed());
        //data.buttons.Set(ButtonType.SubWeapon, playerControls.Player.SubWeapon.IsPressed());
        //data.buttons.Set(ButtonType.MilyWeapon, playerControls.Player.MilyWeapon.IsPressed());
        //data.buttons.Set(ButtonType.BombWeapon, playerControls.Player.BombWeapon.IsPressed());
        //data.buttons.Set(ButtonType.Attack, playerControls.Player.Attack.IsPressed());

        input.Set(playerInput);
        playerInput = default;
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

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            NetworkObject playerObject = runner.Spawn(testPlayer, spawnPoint.position, spawnPoint.rotation, inputAuthority: player);
            runner.SetPlayerObject(player, playerObject);
            playerObjects.Add(player, playerObject);
        }
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
            return;


        if (playerObjects.TryGetValue(player, out NetworkObject playerObject))
        {
            if (playerObject != null)
                runner.Despawn(playerObject);
        }

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

    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }



}
