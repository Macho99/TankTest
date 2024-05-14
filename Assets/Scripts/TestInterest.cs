using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInterest : NetworkBehaviour,IInterestEnter,IInterestExit
{

    public override void FixedUpdateNetwork()
    {
        if (Runner.IsServer)
        {
            var player = Object.InputAuthority;
            if (!player.IsNone)
            {
                Runner.AddPlayerAreaOfInterest(player, transform.position, 5f);
            }
        }
    }

    public void InterestEnter(PlayerRef player)
    {
        Debug.Log("InterestEnter : " + player);
    }

    public void InterestExit(PlayerRef player)
    {
        Debug.Log("InterestExit : " + player);
    }
}
