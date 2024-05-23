using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UseBehavior : NetworkBehaviour
{
    public abstract bool Use(PlayerStat stat, int health);

}
