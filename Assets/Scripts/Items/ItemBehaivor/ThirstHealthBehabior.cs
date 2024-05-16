using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirstHealthBehabior : UseBehavior
{
    public override bool Use(PlayerStat stat, int health)
    {
        if (!stat.Health(PlayerStatType.ThirstGauge, health))
            return false;

        return true;

    }
}
