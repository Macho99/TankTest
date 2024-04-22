using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
	private static GameManager instance;
	private static PoolManager poolManager;
	private static ResourceManager resourceManager;
	private static UIManager uiManager;

    public static GameManager Instance { get { return instance; } }
	public static PoolManager Pool { get { return poolManager; } }
	public static ResourceManager Resource { get { return resourceManager; } }
	public static UIManager UI { get { return uiManager; } }

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
    }
}