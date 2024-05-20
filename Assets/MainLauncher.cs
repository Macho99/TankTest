using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;

public class MainLauncher : MonoBehaviour
{

    private void OnEnable()
    {
        GameManager.network.onRunnerAction += SetupEvent;
    }
    private void OnDisable()
    {
        GameManager.network.onRunnerAction -= SetupEvent;
    }


    private void SetupEvent(NetworkRunner runner)
    {
        if (runner == null)
        {

            return;
        }

        NetworkEvents events = runner.GetComponent<NetworkEvents>();
        if (events != null)
        {
            events.PlayerJoined.AddListener(OnPlayerJoin);
            events.PlayerLeft.AddListener(OnPlayerLeft);
        }



    }
    private void OnPlayerJoin(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            NetworkObject roomPlayerPrefab = GameManager.Resource.Load<NetworkObject>("Player/RoomPlayer");
            NetworkObject roomPlayer = runner.Spawn(roomPlayerPrefab, Vector3.zero, Quaternion.identity, player);
            runner.SetPlayerObject(player, roomPlayer);
            runner.MakeDontDestroyOnLoad(roomPlayer.gameObject);
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

}
