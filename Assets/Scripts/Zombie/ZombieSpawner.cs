using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using TMPro;
using UnityEngine.InputSystem;
using Fusion.Addons.SimpleKCC;


[DefaultExecutionOrder(-10)]
public class ZombieSpawner : SimulationBehaviour, INetworkRunnerCallbacks, IBeforeUpdate
{
	[SerializeField] bool spawn;
	[SerializeField] Transform playerSpawnPoint;
	[SerializeField] NetworkPrefabRef playerPrefab;
	[SerializeField] NetworkPrefabRef zombiePrefab;
	[SerializeField] TextMeshProUGUI connectInfoText;
	[SerializeField] float spawnInterval = 2f;

	TestInputData accumInput;
	Vector2Accumulator lookAccum = new Vector2Accumulator(0.02f, true);

	private NetworkRunner runner;
	private TickTimer timer;
	private bool isFirst = true;

	private void OnEnable()
	{
		StartGame(GameMode.AutoHostOrClient);
	}

	async void StartGame(GameMode mode)
	{
		// Create the Fusion runner and let it know that we will be providing user input
		runner = gameObject.AddComponent<NetworkRunner>();
		runner.ProvideInput = true;

		// Create the NetworkSceneInfo from the current scene
		var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
		var sceneInfo = new NetworkSceneInfo();
		if (scene.IsValid)
		{
			sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
		}

		// Start or join (depends on gamemode) a session with a specific name
		await runner.StartGame(new StartGameArgs()
		{
			GameMode = mode,
			SessionName = $"TestRoom1",
			Scene = scene,
			SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
		});

		if (Runner.IsServer)
		{
			connectInfoText.text = "호스트로 연결됨";
		}
		else
		{
			connectInfoText.text = "클라이언트로 연결됨";
		}


		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	public override void FixedUpdateNetwork()
	{
		if (Runner.IsServer == false) return;
		if(spawn == false) return;
		if (timer.ExpiredOrNotRunning(Runner) == false) return;

		timer = TickTimer.CreateFromSeconds(Runner, spawnInterval);

		if (isFirst == true)
		{
			isFirst = false;

			for(int i = 0; i < 10; i++)
			{
				SpawnZombie();
			}
			return;
		}
	}

	public void SpawnZombie(NetworkRunner.OnBeforeSpawned beforeSpawned = null)
	{
		if(beforeSpawned == null)
		{
			beforeSpawned = BeforeSpawned;
		}
		runner.Spawn(zombiePrefab, onBeforeSpawned: beforeSpawned);
	}

	private void BeforeSpawned(NetworkRunner runner, NetworkObject netObj)
	{
		Random.InitState(runner.SessionInfo.Name.GetHashCode() * netObj.Id.Raw.GetHashCode());

		Vector3 pos = Random.insideUnitSphere * 10f;
		pos.y = 0f;
		Zombie zombie = netObj.GetComponent<Zombie>(); 
		zombie.transform.rotation = Quaternion.LookRotation(new Vector3(Random.value, 0f, Random.value));
		zombie.transform.position = transform.position + pos;
	}

	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{

	}

	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{

	}

	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
	{
		if (runner.IsServer == false) return;
		runner.Spawn(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation, inputAuthority: player);
	}

	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{

	}

	public void BeforeUpdate()
	{
		// Enter key is used for locking/unlocking cursor in game view.
		var keyboard = Keyboard.current;
		if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
		{
			if (Cursor.lockState == CursorLockMode.Locked)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			else
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}

		// Accumulate input only if the cursor is locked.
		if (Cursor.lockState != CursorLockMode.Locked)
			return;

		var mouse = Mouse.current;
		if (mouse != null)
		{
			Vector2 mouseDelta = mouse.delta.ReadValue();
			lookAccum.Accumulate(mouseDelta);
			accumInput.buttons.Set(Buttons.Fire, mouse.leftButton.isPressed);
		}

		if (keyboard != null)
		{
			Vector2 moveDirection = Vector2.zero;

			if (keyboard.wKey.isPressed) { moveDirection += Vector2.up; }
			if (keyboard.sKey.isPressed) { moveDirection += Vector2.down; }
			if (keyboard.aKey.isPressed) { moveDirection += Vector2.left; }
			if (keyboard.dKey.isPressed) { moveDirection += Vector2.right; }

			accumInput.buttons.Set(Buttons.Jump, keyboard.spaceKey.isPressed);
			accumInput.buttons.Set(Buttons.Interact, keyboard.fKey.isPressed);

			accumInput.moveVec = moveDirection.normalized;
		}
	}

	public void OnInput(NetworkRunner runner, NetworkInput input)
	{
		accumInput.lookVec = lookAccum.ConsumeTickAligned(runner);
		input.Set(accumInput);
	}

	public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
	{

	}

	public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
	{

	}

	public void OnConnectedToServer(NetworkRunner runner)
	{
	}

	public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
	{

	}

	public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
	{

	}

	public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
	{

	}

	public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
	{

	}

	public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
	{

	}

	public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
	{

	}

	public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
	{

	}

	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data)
	{

	}

	public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
	{

	}

	public void OnSceneLoadDone(NetworkRunner runner)
	{
	}

	public void OnSceneLoadStart(NetworkRunner runner)
	{
	}
}
