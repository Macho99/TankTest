using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Random = UnityEngine.Random;
public enum ItemBoxType { WeaponBox, UseItemBox, AmmoBox, Size }
public class ItemBoxSpawner : NetworkBehaviour
{
    [SerializeField] private ItemBoxRandomSpawnData[] randomSpawnData;
    [SerializeField] private Transform[] randomSpawnpoint;

    private float accumulatedWeights;
    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            CalculateWeight();
            int count = 0;
            for (int i = 0; i < randomSpawnpoint.Length; i++)
            {
                SpawnItem(randomSpawnpoint[i].position, randomSpawnpoint[i].rotation);
            }
        }
    }

    public void CalculateWeight()
    {
        accumulatedWeights = 0;
        for (int i = 0; i < randomSpawnData.Length; i++)
        {
            accumulatedWeights += randomSpawnData[i].chance;
            randomSpawnData[i].weight = accumulatedWeights;
        }
    }
    public void SpawnItem(Vector3 position, Quaternion rotation)
    {
        var clone = randomSpawnData[GetRandomIndex()];
        Runner.Spawn(clone.itemBox, position, rotation);
    }
    public int GetRandomIndex()
    {
        float random = Random.value * accumulatedWeights;
        for (int i = 0; i < randomSpawnData.Length; i++)
        {
            if (randomSpawnData[i].weight >= random)
            {
                return i;
            }
        }
        return 0;
    }
}

[Serializable]
public class ItemBoxRandomSpawnData
{
    public InteractItemBox itemBox;
    public ItemBoxType type;

    [Range(1, 100)] public int chance;
    [HideInInspector] public float weight;

}