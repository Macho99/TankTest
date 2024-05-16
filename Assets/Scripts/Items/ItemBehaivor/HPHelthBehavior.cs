using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPHelthBehavior : UseBehavior
{
    public override bool Use(PlayerStat stat, int health)
    {
        if (stat.Health(PlayerStatType.HPGauge, health))
            return true;

        return false;
    }

}
