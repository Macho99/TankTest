using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Use", menuName = "SO/ItemSO/UseItem")]
public class UsableItemSO : ItemSO
{
    [SerializeField] private int health;

    public int Helath { get => health; }
}

