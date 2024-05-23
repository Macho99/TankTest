using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Ammo : Item
{
    public void Use(int count = 1)
    {
        if (currentCount <= 0)
            return;

        currentCount -= count;
        if (currentCount <= 0)
        {
            if (HasStateAuthority)
            {
                Runner.Despawn(Object);
            }
        }
    }


}
