using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractState : PlayerState
{

    public PlayerInteractState(PlayerController owner) : base(owner)
    {
    }

    public void Init(int interactIndex)
    {

    }
    public override void Enter()
    {
        owner.animator.SetFloat("InteractType", (float)owner.interact.InteractInfo.interactType);

    }

    public override void Exit()
    {

    }

    public override void FixedUpdateNetwork()
    {
    }

    public override void Transition()
    {
    }

}
