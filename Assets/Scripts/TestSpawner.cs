using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;
using UnityEngine.SceneManagement;
using Fusion.Addons.SimpleKCC;
using TMPro;
using UnityEngine.InputSystem;

public class TestSpawner : MonoBehaviour
{
    NetworkRunner runner;
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] NetworkPrefabRef zombiePrefab;
    [SerializeField] Transform spawnPoint;

    bool spawned = false;
    private PlayerControls playerControls;
    NetworkInputData playerInput = new NetworkInputData();
    Vector2Accumulator lookAccum = new Vector2Accumulator(0.02f, true);

    private void Awake()
    {

        NetworkRunner runner = FindObjectOfType<NetworkRunner>();
        if (runner == null || runner.State == NetworkRunner.States.Shutdown)
        {
            Debug.Log(GameManager.network.Runner);
            StarGame(runner);
            lookAccum.Accumulate(Mouse.current.delta.ReadValue());
            return;
        }

        Debug.Log("생성안함");
        SetupEvent(runner);

    }
    public async void StarGame(NetworkRunner runner)
    {

        runner = GameManager.Resource.Instantiate<NetworkRunner>("Other/Runner");
        runner.name = "테스트 스포너";
        GameManager.network.Runner = runner;
        NetworkEvents networkEvents = runner.GetComponent<NetworkEvents>();
        SetupEvent(runner);

        StartGameArgs startGameArgs = new StartGameArgs();
        startGameArgs.GameMode = GameMode.AutoHostOrClient;
        startGameArgs.SessionName = "Test";
        startGameArgs.Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        runner.ProvideInput = true;
        Debug.Log("생성");
        await runner.StartGame(startGameArgs);
        return;

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


    public void SpawnZombie(NetworkRunner.OnBeforeSpawned beforeSpawned = null)
    {
        if (beforeSpawned == null)
        {
            beforeSpawned = BeforeSpawned;
        }
        runner.Spawn(zombiePrefab, onBeforeSpawned: beforeSpawned);
    }

    private void BeforeSpawned(NetworkRunner runner, NetworkObject netObj)
    {
        Random.InitState(runner.SessionInfo.Name.GetHashCode() * netObj.Id.Raw.GetHashCode());

        Vector3 pos = Random.insideUnitSphere * 100f;
        pos.y = 0f;
        Zombie zombie = netObj.GetComponent<Zombie>();
        zombie.transform.rotation = Quaternion.LookRotation(new Vector3(Random.value, 0f, Random.value));
        zombie.transform.position = transform.position + pos;
    }

    private void SetupEvent(NetworkRunner runner)
    {
        if (runner == null)
        {
            Debug.Log("널");
            return;
        }

        NetworkEvents events = runner.GetComponent<NetworkEvents>();
        if (events != null)
        {
            events.PlayerJoined.AddListener(OnPlayerJoined);
            events.PlayerLeft.AddListener(OnPlayerLeft);
            events.OnInput.AddListener(OnInput);

            Debug.Log(events);
            return;
        }



    }


    private void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        if (runner.TryGetPlayerObject(player, out NetworkObject networkObject))
        {
            Debug.Log(player);

            runner.Despawn(networkObject);
        }
    }


    void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        NetworkObject playerPrefab = GameManager.Resource.Load<NetworkObject>("Player/Player");
        if (runner.IsServer)
        {
            NetworkObject playerObject = runner.Spawn(playerPrefab, spawnPoint.position, spawnPoint.rotation, inputAuthority: player);
            runner.SetPlayerObject(player, playerObject);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

        playerInput.inputDirection = playerControls.Player.Move.ReadValue<Vector2>();
        playerInput.buttons.Set(ButtonType.Run, playerControls.Player.Run.IsPressed());
        playerInput.buttons.Set(ButtonType.Jump, playerControls.Player.Jump.IsPressed());
        playerInput.buttons.Set(ButtonType.Crouch, playerControls.Player.Crouch.IsPressed());
        playerInput.buttons.Set(ButtonType.Interact, playerControls.Player.Interact.IsPressed());
        if (!GameManager.UI.MenuOpened)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerInput.buttons.Set(ButtonType.MouseLock, playerControls.Player.TestMouseCursurLock.IsPressed());
            playerInput.buttons.Set(ButtonType.Adherence, playerControls.Player.Adherence.IsPressed());
            playerInput.mouseDelta = lookAccum.ConsumeTickAligned(runner);

        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }
        playerInput.buttons.Set(ButtonType.ActiveItemContainer, playerControls.Player.ActiveItemContainer.IsPressed());
        playerInput.buttons.Set(ButtonType.PutWeapon, playerControls.Player.PutWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.FirstWeapon, playerControls.Player.FirstWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.SecondWeapon, playerControls.Player.SecondWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.SubWeapon, playerControls.Player.SubWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.MilyWeapon, playerControls.Player.MilyWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.BombWeapon, playerControls.Player.BombWeapon.IsPressed());
        playerInput.buttons.Set(ButtonType.Attack, playerControls.Player.Attack.IsPressed());
        playerInput.buttons.Set(ButtonType.Reload, playerControls.Player.Reload.IsPressed());

        input.Set(playerInput);
        playerInput = default;
    }

    private void Update()
    {
        lookAccum.Accumulate(Mouse.current.delta.ReadValue());

    }


}
