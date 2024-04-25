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
    private float currentTIme;

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

    }
    public override void Render()
    {


    }
    public override void FixedUpdateNetwork()
    {

        if (currentTick.IsRunning)
        {
            if (!currentTick.Expired(Runner))
                return;

            currentTick = TickTimer.None;
            operationUI?.CloseUI();
            operationUI = null;

        }



    }
    public override void Detect(out InteractInfo interactInfo)
    {
        interactInfo = this.info;
    }

    public override void Interact(PlayerController player, out InteractObject interactObject)
    {
        if (IsFarming())
        {
            interactObject = null;
            return;
        }



        // player.stateMachine.ChangeState(PlayerController.PlayerState.Interact);

        if (player.HasInputAuthority)
        {
            if (operationUI == null)
            {
                operationUI = GameManager.UI.ShowInGameUI<OperationUI>("UI/PlayerUI/OperationUI");
                operationUI.SetTarget(this.transform);
                operationUI.SetOffset(new Vector3(0f, 200f, 0f));
                player.AddDebugText("uiopen");
            }     
        }
        currentTick = TickTimer.CreateFromSeconds(Runner, farmingTime);

        interactObject = this;

    }

    public void Progress()
    {
        if (currentTick.IsRunning)
        {
            float percent = (farmingTime - (float)currentTick.RemainingTime(Runner)) / farmingTime;
            float test = Mathf.Floor(percent * 100f);

            if (operationUI != null)
                operationUI.SetProgress(test);
        }
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
        currentTick = TickTimer.None;
        operationUI.CloseUI();
    }
}
