using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;
using UnityEngine.SceneManagement;

public class TestSpawner : MonoBehaviour
{
    private NetworkRunner runner;
    private void OnEnable()
    {
        StartGame(GameMode.AutoHostOrClient);
    }

    async void StartGame(GameMode mode)
    {
        // Create the Fusion runner and let it know that we will be providing user input
        runner = gameObject.GetComponent<NetworkRunner>();
        runner.ProvideInput = true;
        // Create the NetworkSceneInfo from the current scene
  
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene,localPhysicsMode:LocalPhysicsMode.Physics3D ,loadSceneMode: LoadSceneMode.Additive);
        }

        // Start or join (depends on gamemode) a session with a specific name
        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = $"TestRoom1",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
