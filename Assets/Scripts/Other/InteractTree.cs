using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractTree : InteractObject
{
    private float farmingCooldown;

    private ItemData item;

    [Networked] private TickTimer currentCooldown { get; set; }
    private float currentTIme;

    private void Awake()
    {
        farmingCooldown = 5f;
        targetTime = 5f;
        state = InteractState.None;
    }
    private void Start()
    {


    }
    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            isUsed = false;
        }
    }
    public override void Render()
    {

    }
    public override void FixedUpdateNetwork()
    {

        if (currentProgress.IsRunning)
        {
            if (!currentProgress.Expired(Runner))
                return;


            Stop();

        }
    }
    public override void Detect(out InteractInfo interactInfo)
    {
        interactInfo = this.info;
    }

    public override bool Interact(PlayerController player, out InteractObject interactObject)
    {
        if (isUsed == true)
        {
            interactObject = null;
            return false;
        }


        interactObject = this;
        isUsed = true;

        return true;
    }

    //public void Progress()
    //{
    //    currentProgress = TickTimer.CreateFromSeconds(Runner, targetTime);
    //    state = InteractState.Progress;


    //}
    public bool IsFarming()
    {
        return currentProgress.IsRunning;
    }
    public bool IsCooldown()
    {
        return currentCooldown.IsRunning;
    }

    public override void Stop()
    {
        isUsed = false;
        currentProgress = TickTimer.None;
        state = InteractState.End;
    }

}
