using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmingObject : InteractObject
{
    private float farmingCooldown;
    private float farmingTime;
    [Networked] private TickTimer currentTick { get; set; }
    [Networked] private TickTimer currentCooldown { get; set; }
    [SerializeField] private InteractInfo interactInfo;


    private void Awake()
    {
        farmingCooldown = 5f;
    }
    public override void Spawned()
    {


    }

    public override void FixedUpdateNetwork()
    {
        if (currentTick.IsRunning)
        {
            if (currentTick.Expired(Runner))
            {

            }
        }
    }
    public override void Detect(out InteractInfo interactInfo)
    {
        interactInfo = this.interactInfo;
    }

    public override void Interact()
    {
        if (IsFarming())
            return;

        if (IsCooldown())
            return;


        currentTick = TickTimer.CreateFromSeconds(Runner, farmingTime);


    }

    public bool IsFarming()
    {
        return currentTick.IsRunning;
    }
    public bool IsCooldown()
    {
        return currentCooldown.IsRunning;
    }
}
