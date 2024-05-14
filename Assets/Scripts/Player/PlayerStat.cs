using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : NetworkBehaviour
{
    [Networked] public PlayerStatus playerStatus { get; private set; }


}


public struct PlayerStatus : INetworkStruct
{
    public int level;
    public int currenthp;
    public int maxHp;
}