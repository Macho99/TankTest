using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using TMPro;
using UnityEngine.InputSystem;

public class ZombieSpawner : SimulationBehaviour, INetworkRunnerCallbacks
{
	[SerializeField] bool spawn;
	[SerializeField] NetworkPrefabRef playerPrefab;
	[SerializeField] NetworkPrefabRef zombiePrefab;
	[SerializeField] NetworkObject target;
	[SerializeField] TextMeshProUGUI connectInfoText;
	[SerializeField] float spawnInterval = 2f;

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
		gameObject.AddComponent<HitboxManager>();

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
			SessionName = "TestRoom",
			Scene = scene,
			SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
		});

		if (runner.IsServer)
		{
			connectInfoText.text = "호스트로 연결됨";
		}
		else
		{
			connectInfoText.text = "클라이언트로 연결됨";
		}
	}

	public override void FixedUpdateNetwork()
	{
		if(spawn == false) return;
		if (timer.ExpiredOrNotRunning(Runner) == false) return;

		timer = TickTimer.CreateFromSeconds(Runner, spawnInterval);

		if (isFirst == true)
		{
			isFirst = false;
			return;
		}
		runner.Spawn(zombiePrefab, onBeforeSpawned: BeforeSpawned);
	}

	private void BeforeSpawned(NetworkRunner runner, NetworkObject netObj)
	{
		Vector3 pos = Random.insideUnitSphere * 10f;
		pos.y = 0f;
		Zombie zombie = netObj.GetComponent<Zombie>();
		zombie.Init(target);
		zombie.Position = transform.position + pos;
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

		runner.Spawn(playerPrefab, transform.position, inputAuthority: player);
	}

	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
	{

	}

	public void OnInput(NetworkRunner runner, NetworkInput input)
	{
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
