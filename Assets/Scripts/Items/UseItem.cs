using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItem : Item
{
    private UseBehavior useBehavior;


    private void Awake()
    {
        useBehavior = GetComponent<UseBehavior>();
    }
    public void Use(PlayerStat playerStat)
    {

        if (useBehavior.Use(playerStat, ((UsableItemSO)itemData).Helath) == true)
        {
            currentCount--;
        }
        if (currentCount == 0)
        {
            if (HasStateAuthority)
                Runner.Despawn(Object);
        }
    }

}
