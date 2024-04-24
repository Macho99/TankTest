using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractTree : InteractObject
{
    private float farmingCooldown;
    private float farmingTime;

    private ItemData item;
    [Networked] private TickTimer currentTick { get; set; }
    [Networked] private TickTimer currentCooldown { get; set; }
    private OperationUI operationUI;
    private bool isUsed;

    private void Awake()
    {
        farmingCooldown = 5f;
        farmingTime = 5f;
    }
    private void Start()
    {

     
    }
    public override void Spawned()
    {
        operationUI = GameManager.UI.ShowInGameUI<OperationUI>("UI/PlayerUI/OperationUI");
        operationUI.SetTarget(this.transform);
        operationUI.SetOffset(new Vector3(0f, 200f, 0f));
    }
    public override void FixedUpdateNetwork()
    {
        if (currentTick.IsRunning)
        {
            float percent = (farmingTime - (float)currentTick.RemainingTime(Runner)) / farmingTime;
            float truncatedPercent = Mathf.Floor(percent * 100f);
            operationUI.SetProgress(truncatedPercent);
        }

        if (isUsed)
        {
            if (!currentTick.Expired(Runner))
                return;

            isUsed = false;
            currentTick = TickTimer.None;
            operationUI.CloseUI();

        }
     


    }
    public override void Detect(out InteractInfo interactInfo)
    {
        interactInfo = this.info;
    }

    public override void Interact(PlayerController player, out InteractObject interactObject)
    {
        if (IsFarming() || IsCooldown() || isUsed)
        {
            interactObject = null;
            return;
        }

        Debug.Log("use");

        isUsed = true;
        // player.stateMachine.ChangeState(PlayerController.PlayerState.Interact);
        currentTick = TickTimer.CreateFromSeconds(Runner, farmingTime);

        interactObject = this;

    }

    public bool IsFarming()
    {
        return currentTick.IsRunning;
    }
    public bool IsCooldown()
    {
        return currentCooldown.IsRunning;
    }

    public override void Stop()
    {
        isUsed = false;
        currentTick = TickTimer.None;
        operationUI.CloseUI();
    }
}
