using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.HID;
using UnityEngine.SceneManagement;

public class LoadSceneManager : NetworkSceneManagerDefault
{
    private NetworkManager networkManager;
    private void Awake()
    {

    }
    public override void Shutdown()
    {

    }
    public void Init(NetworkManager networkManager)
    {
        this.networkManager = networkManager;
    }
    public void OnBeforeSpawned(NetworkRunner runner, NetworkObject obj)
    {
        obj.gameObject.SetActive(false);

    }
    public async Task LoadGameScene(SceneRef sceneRef)
    {

        if (Runner == null)
            return;
        if (!Runner.IsServer)
            return;
        Runner.SessionInfo.IsOpen = false;
        await Runner.LoadScene(sceneRef, LoadSceneMode.Single);
    

        List<(PlayerRef, NetworkObject)> players = new List<(PlayerRef, NetworkObject)>();
        PlayerPreviewController preview = FindObjectOfType<PlayerPreviewController>();



    }
    protected override void OnLoadSceneProgress(SceneRef sceneRef, float progress)
    {

        base.OnLoadSceneProgress(sceneRef, progress);

    }
    protected override IEnumerator LoadSceneCoroutine(SceneRef sceneRef, NetworkLoadSceneParameters sceneParams)
    {
        yield return base.LoadSceneCoroutine(sceneRef, sceneParams);




    }


    protected override IEnumerator OnSceneLoaded(SceneRef sceneRef, Scene scene, NetworkLoadSceneParameters sceneParams)
    {

        yield return base.OnSceneLoaded(sceneRef, scene, sceneParams);

        yield return null;

        if (sceneRef.AsIndex == SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/GameScene.unity"))
        {
            if (scene == SceneManager.GetActiveScene())
            {
                NetworkObject playerPrefab = GameManager.Resource.Load<NetworkObject>("Player/Player");
                foreach (var player in Runner.ActivePlayers)
                {
                    if (Runner.TryGetPlayerObject(player, out NetworkObject playerObj))
                    {
                       
                        NetworkObject newPlayer = Runner.Spawn(playerPrefab, inputAuthority: player);
                        Runner.SetPlayerObject(player, newPlayer);
                        Runner.Despawn(playerObj);
                    }

                }
                Debug.Log(Runner.ActivePlayers.ToArray().Length);
            }
        }
    }

}
