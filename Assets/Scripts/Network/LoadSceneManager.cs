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
  

    private Dictionary<PlayerRef, int[]> decoDic = new Dictionary<PlayerRef, int[]>();
    [SerializeField] private Vector3 spawnPoint;
    [SerializeField] private Quaternion spawnRotation;
    private void Awake()
    {
       
    }
    public override void Shutdown()
    {

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

       

        await Runner.LoadScene(GameManager.network.GetSceneRef(SceneType.LoadingScene), LoadSceneMode.Additive, setActiveOnLoad: true);
        await Runner.UnloadScene(GameManager.network.GetSceneRef(SceneType.RoomScene));

        await Runner.LoadScene(GameManager.network.GetSceneRef(SceneType.GameScene), LoadSceneMode.Additive, setActiveOnLoad: true);
        await Runner.UnloadScene(GameManager.network.GetSceneRef(SceneType.LoadingScene));


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

        Runner.SessionInfo.IsOpen = true;
        Runner.SessionInfo.IsVisible = true;    
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

        Debug.Log(GameManager.network);
        if(GameManager.network != null)
        {
            Debug.Log("asdsad");
            if (sceneRef == GameManager.network.GetSceneRef(SceneType.LoadingScene))
            {
                SceneManager.UnloadSceneAsync(GameManager.network.GetSceneRef(SceneType.StartScene).AsIndex);
            }
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
