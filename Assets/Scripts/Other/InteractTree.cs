using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractTree : InteractObject
{
    private float farmingCooldown;



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
        Debug.Log("complete");

        if (currentProgress.IsRunning)
        {
            if (!currentProgress.Expired(Runner))
                return;
            else
            {
                base.Complete();
                Stop();
            }
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
            Debug.Log("empty");
            return false;
        }



        isUsed = true;
        interactObject = this;

        return true;
    }


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

    public override void Result()
    {
        Stop();




        // 아이템 보상 예정
        return;

    }
}
