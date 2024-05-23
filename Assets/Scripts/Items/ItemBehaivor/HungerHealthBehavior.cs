using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungerHealthBehavior : UseBehavior
{
    public override bool Use(PlayerStat stat, int health)
    {
        if (stat.Health(PlayerStatType.HPGauge, health))
            return true;

        return false;
    }

   
}
