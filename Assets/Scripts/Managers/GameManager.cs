using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    const string feedbackPath = "Managers/FeedbackManager";
    const string networkPath = "Managers/NetworkManager";

    private static GameManager instance;
    private static PoolManager poolManager;
    private static ResourceManager resourceManager;
    private static UIManager uiManager;
    private static FeedbackManager feedbackManager;
    private static NetworkManager networkManager;
    private static AuthManager authManager;
    private static DataManager dataManager;
    private static SoundManager soundManager;

    private int normalZombieCnt;
    private int bruteZombieCnt;
    private int wretchZombieCnt;
    public int NormalZombieCnt { get=>normalZombieCnt; set { normalZombieCnt = value; onChangeNormalZombieCnt?.Invoke(normalZombieCnt); } }
    public int BruteZombieCnt { get => bruteZombieCnt; set { bruteZombieCnt = value; onChangeBruteZombieCnt?.Invoke(bruteZombieCnt); } }
    public int WretchZombieCnt { get => wretchZombieCnt; set { wretchZombieCnt = value; onChangeWretchZombieCnt?.Invoke(wretchZombieCnt); } }

    public event Action<int> onChangeNormalZombieCnt;
    public event Action<int> onChangeBruteZombieCnt;
    public event Action<int> onChangeWretchZombieCnt;
    public static GameManager Instance { get { return instance; } }
    public static PoolManager Pool { get { return poolManager; } }
    public static ResourceManager Resource { get { return resourceManager; } }
    public static UIManager UI { get { return uiManager; } }
    public static WeatherManager Weather { get; set; }
    public static FeedbackManager Feedback { get { return feedbackManager; } }
    public static NetworkManager network { get { return networkManager; } }
    public static SoundManager Sound { get { return soundManager; } }

    public static AuthManager auth { get { return authManager; } }
    public static DataManager data { get { return dataManager; } }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
        InitManagers();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private void InitManagers()
    {
        resourceManager = new GameObject("ResourceManager").AddComponent<ResourceManager>();
        resourceManager.transform.parent = transform;

        poolManager = new GameObject("PoolManager").AddComponent<PoolManager>();
        poolManager.transform.parent = transform;

        uiManager = new GameObject("UIManager").AddComponent<UIManager>();
        uiManager.transform.parent = transform;

        feedbackManager = Resource.Instantiate<FeedbackManager>(feedbackPath, transform);

        networkManager = Resource.Instantiate<NetworkManager>(networkPath, transform);
        networkManager.transform.parent = transform;

        authManager = new GameObject("AuthManager").AddComponent<AuthManager>();
        authManager.transform.parent = transform;

        dataManager = new GameObject("DataManager").AddComponent<DataManager>();
        dataManager.transform.parent = transform;

        soundManager = new GameObject("SoundManager").AddComponent<SoundManager>();
        soundManager.transform.parent = transform;
    }
}