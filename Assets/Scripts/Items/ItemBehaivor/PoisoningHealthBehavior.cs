using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisoningHealthBehavior : UseBehavior
{
    public override bool Use(PlayerStat stat, int health)
    {
        if(!stat.Health(PlayerStatType.PoisoningGauge, health))
            return false;

        return true;    
    }

}
