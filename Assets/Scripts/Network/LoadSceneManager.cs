using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.HID;
using UnityEngine.SceneManagement;

public class LoadSceneManager : NetworkSceneManagerDefault
{
    [SerializeField] private GameObject ingamePlayerPrefab;
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
    public async Task LoadGameScene(SceneRef sceneRef)
    {

        if (Runner == null)
            return;
        if (!Runner.IsServer)
            return;

        Runner.SessionInfo.IsOpen = false;
        FindObjectOfType<AudioListener>().enabled = false;
        FindObjectOfType<EventSystem>().enabled = false;
        Debug.Log("state");
        List<(PlayerRef, NetworkObject)> players = new List<(PlayerRef, NetworkObject)>();
        PlayerPreviewController preview = FindObjectOfType<PlayerPreviewController>();
        GameObject newPlayer = preview.transform.Find("PlayerPreset").gameObject;
        foreach (var player in Runner.ActivePlayers)
        {
            if (Runner.TryGetPlayerObject(player, out NetworkObject playerObject))
            {
                RoomPlayer roomPlayer = playerObject.GetComponent<RoomPlayer>();


                NetworkObject networkObject = Runner.Spawn(newPlayer, Vector3.zero, Quaternion.identity, player);
                networkObject.name = player.ToString();
                Transform[] allChildren = networkObject.transform.GetComponentsInChildren<Transform>();
                foreach (Transform child in allChildren)
                {
                    if (child.name == transform.name)
                        continue;

                    if (child.name == "Hair")
                    {
                        for (int i = 0; i < child.childCount; i++)
                        {
                            child.GetChild(i).gameObject.SetActive(i == roomPlayer.HairIndex);
                        }

                    }
                    else if (child.name == "Beard")
                    {
                        for (int i = 0; i < child.childCount; i++)
                        {
                            child.GetChild(i).gameObject.SetActive(i == roomPlayer.BreardIndex);
                        }
                    }
                    else if (child.name == "Preset")
                    {
                        for (int i = 0; i < child.childCount; i++)
                        {
                            if (i == roomPlayer.presetIndex)
                            {
                                Transform preset = child.GetChild(roomPlayer.presetIndex);
                                preset.GetComponent<SkinnedMeshRenderer>().material = preview.GetMaterial(roomPlayer.ColorIndex);
                                preset.gameObject.SetActive(true);
                            }
                            else
                            {
                                child.GetChild(i).gameObject.SetActive(false);
                            }

                        }

                    }
                }
                networkManager.RemovePlayer(player);
                MakeDontDestroyOnLoad(networkObject.gameObject);
                players.Add((player, networkObject));

            }
        }

        await Runner.LoadScene(sceneRef, LoadSceneMode.Single);

        foreach (var player in players)
        {
            Debug.Log(player);
            Runner.SetPlayerObject(player.Item1, player.Item2);
        }



        networkManager.CurrentPlace = NetworkManager.PlaceType.Loading;

        SceneRef gameSceneRef = SceneRef.FromIndex(2);

        await Runner.LoadScene(gameSceneRef, LoadSceneMode.Additive);

        await Runner.UnloadScene(sceneRef);



    }



    protected override void OnLoadSceneProgress(SceneRef sceneRef, float progress)
    {
        base.OnLoadSceneProgress(sceneRef, progress);

    }
    protected override IEnumerator LoadSceneCoroutine(SceneRef sceneRef, NetworkLoadSceneParameters sceneParams)
    {
        return base.LoadSceneCoroutine(sceneRef, sceneParams);
    }

    protected override IEnumerator OnSceneLoaded(SceneRef sceneRef, Scene scene, NetworkLoadSceneParameters sceneParams)
    {

        return base.OnSceneLoaded(sceneRef, scene, sceneParams);
    }

}
