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

    private Dictionary<PlayerRef, int[]> decoDic = new Dictionary<PlayerRef, int[]>();
    [SerializeField] private Vector3 spawnPoint;
    [SerializeField] private Quaternion spawnRotation;
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UnloadScene()
    {

    }
    public async Task LoadGameScene()
    {

        if (Runner == null)
            return;

        RPC_UnloadScene();
        if (!Runner.IsServer)
            return;


        Runner.SessionInfo.IsOpen = false;
        Runner.SessionInfo.IsVisible = false;

       
        await Runner.LoadScene(networkManager.GetSceneRef(SceneType.LoadingScene), LoadSceneMode.Additive, setActiveOnLoad: true);
        await Runner.LoadScene(networkManager.GetSceneRef(SceneType.GameScene), LoadSceneMode.Additive, setActiveOnLoad: true);
        await Runner.UnloadScene(networkManager.GetSceneRef(SceneType.RoomScene));
        await Runner.UnloadScene(networkManager.GetSceneRef(SceneType.LoadingScene));



        NetworkObject playerPrefab = GameManager.Resource.Load<NetworkObject>("Player/Player");
        foreach (var player in Runner.ActivePlayers)
        {
            if (Runner.TryGetPlayerObject(player, out NetworkObject playerObj))
            {
                RoomPlayer roomPlayer = playerObj.GetComponent<RoomPlayer>();

                int[] DecoArray = new int[(int)AppearanceType.Size];
                DecoArray[(int)AppearanceType.Hair] = roomPlayer.HairIndex;
                DecoArray[(int)AppearanceType.Breard] = roomPlayer.BreardIndex;
                DecoArray[(int)AppearanceType.Color] = roomPlayer.ColorIndex;
                DecoArray[(int)AppearanceType.Preset] = roomPlayer.presetIndex;
                decoDic.Add(player, DecoArray);

                NetworkObject newPlayer = Runner.Spawn(playerPrefab, spawnPoint, spawnRotation, inputAuthority: player, onBeforeSpawned: BeforePlayerSpawned);

                print(newPlayer.name);
                Runner.SetPlayerObject(player, newPlayer);
                Runner.Despawn(playerObj);
            }

        }

        Debug.Log(Runner.ActivePlayers.ToArray().Length);
        Runner.SessionInfo.IsOpen = true;
        Runner.SessionInfo.IsVisible = true;
        print("Load");

    }


    protected override void OnLoadSceneProgress(SceneRef sceneRef, float progress)
    {

        base.OnLoadSceneProgress(sceneRef, progress);



    }
    protected override IEnumerator LoadSceneCoroutine(SceneRef sceneRef, NetworkLoadSceneParameters sceneParams)
    {
        yield return base.LoadSceneCoroutine(sceneRef, sceneParams);
     


        Debug.Log(sceneRef);


    }


    protected override IEnumerator OnSceneLoaded(SceneRef sceneRef, Scene scene, NetworkLoadSceneParameters sceneParams)
    {

        yield return base.OnSceneLoaded(sceneRef, scene, sceneParams);

        yield return null;
        if (sceneRef == networkManager.GetSceneRef(SceneType.LoadingScene))
        {
            SceneManager.UnloadSceneAsync(networkManager.GetSceneRef(SceneType.StartScene).AsIndex);
        }
       
    }

    private void BeforePlayerSpawned(NetworkRunner runner, NetworkObject obj)
    {

        PlayerController player = obj.GetComponent<PlayerController>();
        PlayerRef playerRef = obj.InputAuthority;
        if (player != null)
        {
            for (int i = 0; i < (int)AppearanceType.Size; i++)
            {
                player.SetupDecoration((AppearanceType)i, decoDic[playerRef][i]);
            }
        }
    }

}
