using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;

public class ZombieSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
	[SerializeField] Zombie zombiePrefab;
	[SerializeField] Transform target;

	private NetworkRunner _runner;

	private void OnEnable()
	{
		StartGame(GameMode.AutoHostOrClient);
	}

	async void StartGame(GameMode mode)
	{
		// Create the Fusion runner and let it know that we will be providing user input
		_runner = gameObject.AddComponent<NetworkRunner>();
		_runner.ProvideInput = true;

		// Create the NetworkSceneInfo from the current scene
		var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
		var sceneInfo = new NetworkSceneInfo();
		if (scene.IsValid)
		{
			sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
		}

		// Start or join (depends on gamemode) a session with a specific name
		await _runner.StartGame(new StartGameArgs()
		{
			GameMode = mode,
			SessionName = "TestRoom",
			Scene = scene,
			SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
		});

		print(_runner.IsServer);
		if(_runner.IsServer)
		{
			StartCoroutine(CoSpawn());
		}
	}

	private IEnumerator CoSpawn()
	{
		while (true)
		{
			Vector3 pos = Random.insideUnitSphere * 10f;
			pos.y = 0f;
			Zombie zombie = _runner.Spawn(zombiePrefab, pos);
			zombie.Init(target);
			yield return new WaitForSeconds(2f);
		}
	}

	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{

	}

	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
	{

	}

	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
	{

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
		//if (Runner.IsServer)
		{
			_ = StartCoroutine(CoSpawn());
		}
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
