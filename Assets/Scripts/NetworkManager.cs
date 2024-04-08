using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks
{
	[SerializeField] PhotonView testSphere;
	[SerializeField] Zombie zombiePrefab;
	[SerializeField] TextMeshProUGUI infoText;
	[SerializeField] float countdownTime = 5f;
	[SerializeField] float zombieSpawnInterval = 2f;

	private void Start()
	{
		// Normal game mode
		if (PhotonNetwork.InRoom)
		{
			//PhotonNetwork.LocalPlayer.SetLoad(true);
		}
		// Debug game mode
		else
		{
			//infoText.text = "Debug Mode";
			PhotonNetwork.LocalPlayer.NickName = $"DebugPlayer {Random.Range(1000, 10000)}";
			PhotonNetwork.ConnectUsingSettings();
		}
	}

	public override void OnConnectedToMaster()
	{
		RoomOptions options = new RoomOptions() { IsVisible = false };
		PhotonNetwork.JoinOrCreateRoom("DebugRoom", options, TypedLobby.Default);
	}

	public override void OnJoinedRoom()
	{
		infoText.text = PhotonNetwork.IsMasterClient ? "호스트" : "클라이언트";
		StartCoroutine(DebugGameSetupDelay());
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log($"Disconnected : {cause}");
		SceneManager.LoadScene("LobbyScene");
	}

	public override void OnLeftRoom()
	{
		Debug.Log("Left Room");
		PhotonNetwork.LoadLevel("LobbyScene");
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		if (newMasterClient.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
			StartCoroutine(SpawnZombieRoutine());
	}

	public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
	{
		if (changedProps.ContainsKey(CustomProperty.LOAD))
		{
			if (PlayerLoadCount() == PhotonNetwork.PlayerList.Length)
			{
				if (PhotonNetwork.IsMasterClient)
					PhotonNetwork.CurrentRoom.SetLoadTime(PhotonNetwork.Time);
			}
			else
			{
				Debug.Log($"Wait players {PlayerLoadCount()} / {PhotonNetwork.PlayerList.Length}");
				infoText.text = $"Wait players {PlayerLoadCount()} / {PhotonNetwork.PlayerList.Length}";
			}
		}
	}

	public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
	{
		if (propertiesThatChanged.ContainsKey(CustomProperty.LOADTIME))
		{
			StartCoroutine(GameStartTimer());
		}
	}

	IEnumerator GameStartTimer()
	{
		double loadTime = PhotonNetwork.CurrentRoom.GetLoadTime();
		while (countdownTime > PhotonNetwork.Time - loadTime)
		{
			int remainTime = (int)(countdownTime - (PhotonNetwork.Time - loadTime));
			infoText.text = $"All Player Loaded, Start count down : {remainTime + 1}";
			yield return new WaitForEndOfFrame();
		}
		Debug.Log("Game Start!");
		infoText.text = "Game Start!";
		//GameStart();

		yield return new WaitForSeconds(1f);
		infoText.text = "";
	}

	private void GameStart()
	{
		float angularStart = (360.0f / PhotonNetwork.CurrentRoom.PlayerCount) * PhotonNetwork.LocalPlayer.GetPlayerNumber();
		float x = 20.0f * Mathf.Sin(angularStart * Mathf.Deg2Rad);
		float z = 20.0f * Mathf.Cos(angularStart * Mathf.Deg2Rad);
		Vector3 position = new Vector3(x, 0.0f, z);
		Quaternion rotation = Quaternion.Euler(0.0f, angularStart, 0.0f);

		PhotonNetwork.Instantiate("Player", position, rotation, 0);

		if (PhotonNetwork.IsMasterClient)
			StartCoroutine(SpawnZombieRoutine());
	}

	private void DebugGameStart()
	{
		//float angularStart = (360.0f / 8f) * PhotonNetwork.LocalPlayer.GetPlayerNumber();
		//float x = 20.0f * Mathf.Sin(angularStart * Mathf.Deg2Rad);
		//float z = 20.0f * Mathf.Cos(angularStart * Mathf.Deg2Rad);
		//Vector3 position = new Vector3(x, 0.0f, z);
		//Quaternion rotation = Quaternion.Euler(0.0f, angularStart, 0.0f);

		//PhotonNetwork.Instantiate("Player", position, rotation, 0);

		if (PhotonNetwork.IsMasterClient)
			StartCoroutine(SpawnZombieRoutine());
	}

	IEnumerator DebugGameSetupDelay()
	{
		yield return new WaitForSeconds(1f);
		DebugGameStart();
	}

	IEnumerator SpawnZombieRoutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(zombieSpawnInterval);

			object[] data = { (int) PhotonNetwork.Time, testSphere.ViewID };
			Vector3 pos = Random.insideUnitSphere * 7f;
			pos.y = 0f;
			pos += transform.position;

			PhotonNetwork.InstantiateRoomObject("Prefabs/Zombie", pos, 
				Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), data: data);
		}
	}

	private int PlayerLoadCount()
	{
		int loadCount = 0;
		foreach (Player player in PhotonNetwork.PlayerList)
		{
			if (player.GetLoad())
				loadCount++;
		}
		return loadCount;
	}
}
