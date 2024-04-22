using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public enum PlayerState { StandLocomotion, CrouchLocomotion, Jump, Land, Falling }
    private NetworkStateMachine stateMachine;
    public PlayerLocomotion movement { get; private set; }
    public Animator animator { get; private set; }
    private CapsuleCollider myCollider;
    [Networked] public float FallingTime { get; set; }
    [Networked] public float prevGravityDirection { get; private set; }
    [Networked] public float currentGravityDirection { get; private set; }
    private void Awake()
    {
        myCollider = GetComponentInChildren<CapsuleCollider>();
        animator = GetComponent<Animator>();
        stateMachine = GetComponent<NetworkStateMachine>();
        movement = GetComponent<PlayerLocomotion>();
        stateMachine.AddState(PlayerState.StandLocomotion, new PlayerStandLocomotionState(this));
        stateMachine.AddState(PlayerState.CrouchLocomotion, new PlayerCrouchLocomotionState(this));
        stateMachine.AddState(PlayerState.Jump, new PlayerJumpState(this));
        stateMachine.AddState(PlayerState.Land, new PlayerLandState(this));
        stateMachine.AddState(PlayerState.Falling, new PlayerFallingState(this));

        stateMachine.InitState(PlayerState.StandLocomotion);
    }
    public override void Spawned()
    {
        FallingTime = 0f;
    }

    public override void FixedUpdateNetwork()
    {
        Falling();
        movement.Move();

    }
    public void Falling()
    {

        if (!movement.IsGround() && movement.Kcc.RealVelocity.y < 0f)
        {
            FallingTime += Runner.DeltaTime;

            Debug.Log(FallingTime.ToString("F1"));
            if (FallingTime >= 0.8f)
            {

                if (stateMachine.curStateStr != "Falling")
                {
                    stateMachine.ChangeState(PlayerState.Falling);
                    return;
                }
            }

        }
    }

    public bool RaycastGroundCheck()
    {
        if (movement.Kcc.RealVelocity.y < 0f)
        {
            Debug.DrawRay(transform.position, Vector3.down * 0.3f, Color.red, 0.1f);

            if (Physics.Raycast(transform.position, Vector3.down, 0.3f, LayerMask.GetMask("Environment")))
            {
                return true;
            }
        }

        return false;
    }

}
