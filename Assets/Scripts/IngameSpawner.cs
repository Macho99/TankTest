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

public class IngameSpawner : MonoBehaviour
{
    NetworkRunner runner;
    [SerializeField] Transform spawnPoint;
    private void Awake()
    {
        NetworkRunner runner = FindObjectOfType<NetworkRunner>();
        if (runner != null)
        {
            this.runner = runner;
            SetupEvent(runner);
        }
        else
        {

        }


    }
    private void OnDisable()
    {
        GameManager.network.onRunnerAction -= SetupEvent;
    }

    //public void SpawnZombie(NetworkRunner.OnBeforeSpawned beforeSpawned = null)
    //{
    //    if (beforeSpawned == null)
    //    {
    //        beforeSpawned = BeforeSpawned;
    //    }
    //    runner.Spawn(zombiePrefab, onBeforeSpawned: beforeSpawned);
    //}

    //private void BeforeSpawned(NetworkRunner runner, NetworkObject netObj)
    //{
    //    Random.InitState(runner.SessionInfo.Name.GetHashCode() * netObj.Id.Raw.GetHashCode());

    //    Vector3 pos = Random.insideUnitSphere * 10f;
    //    pos.y = 0f;
    //    Zombie zombie = netObj.GetComponent<Zombie>();
    //    zombie.transform.rotation = Quaternion.LookRotation(new Vector3(Random.value, 0f, Random.value));
    //    zombie.transform.position = transform.position + pos;
    //}
    private void SetupEvent(NetworkRunner runner)
    {
        if (runner == null)
        {

            return;
        }

        NetworkEvents events = runner.GetComponent<NetworkEvents>();
        if (events != null)
        {
            //events.PlayerJoined.AddListener(OnPlayerJoined);
            //events.PlayerLeft.AddListener(OnPlayerLeft);
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
 
}
