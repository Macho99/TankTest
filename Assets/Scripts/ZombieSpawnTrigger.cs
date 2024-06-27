using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawnTrigger : MonoBehaviour
{
	IngameSpawner ingameSpawner;

	private void Awake()
	{
		ingameSpawner = GetComponentInParent<IngameSpawner>();
	}

	//private void OnTriggerEnter(Collider other)
	//{
	//	ingameSpawner.SpawnInitZombie();
	//	Destroy(gameObject);
	//}
}
