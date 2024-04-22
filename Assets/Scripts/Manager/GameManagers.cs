using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagers : MonoBehaviour
{
    private static GameManagers instance;


    public static GameManagers Instance { get { return instance; } }
    public AuthManager AuthManager { get; private set; }
    public DataManager DataManager { get; private set; }
    public NetworkManager NetworkManager { get; private set; }
    public UIManagers UIManager { get; private set; }
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);

        AuthManager = GetComponentInChildren<AuthManager>();
        NetworkManager = GetComponentInChildren<NetworkManager>();
        DataManager = GetComponentInChildren<DataManager>();
        UIManager = GetComponentInChildren<UIManagers>();

    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
