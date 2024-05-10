using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public enum PlayerState { StandLocomotion, CrouchLocomotion, Jump, Land, Falling, ClimbPass, Interact }

    public NetworkStateMachine stateMachine { get; private set; }
    public Animator animator { get; private set; }
    public PlayerLocomotion movement { get; private set; }
    public PlayerInteract interact { get; private set; }
    private CapsuleCollider myCollider;
    public PlayerInputListner InputListner { get; private set; }
    public Inventory Inventory { get; private set; }
    private LocalPlayerDebugUI LocaldebugUI;

    [Networked] public float VelocityY { get; set; }
    private void Awake()
    {
        myCollider = GetComponentInChildren<CapsuleCollider>();
        animator = GetComponent<Animator>();
        stateMachine = GetComponent<NetworkStateMachine>();
        movement = GetComponent<PlayerLocomotion>();
        InputListner = GetComponent<PlayerInputListner>();
        interact = GetComponent<PlayerInteract>();
        Inventory = GetComponentInChildren<Inventory>();
        stateMachine.AddState(PlayerState.StandLocomotion, new PlayerStandLocomotionState(this));
        stateMachine.AddState(PlayerState.CrouchLocomotion, new PlayerCrouchLocomotionState(this));
        stateMachine.AddState(PlayerState.Jump, new PlayerJumpState(this));
        stateMachine.AddState(PlayerState.Land, new PlayerLandState(this));
        stateMachine.AddState(PlayerState.Falling, new PlayerFallingState(this));
        stateMachine.AddState(PlayerState.Interact, new PlayerInteractState(this));

        stateMachine.InitState(PlayerState.StandLocomotion);

    }
    public override void Spawned()
    {
        VelocityY = 0f;
        name = $"{Object.InputAuthority} ({(HasInputAuthority ? "Input Authority" : (HasStateAuthority ? "State Authority" : "Proxy"))})";




    }

    public override void FixedUpdateNetwork()
    {
        Falling();

        movement.Move();


       

    }
   
    public void Falling()
    {


        animator.SetFloat("VelocityY", VelocityY);

        if (movement.Kcc.RealVelocity.y <= -1F)
        {
            VelocityY += Mathf.Abs(movement.Kcc.RealVelocity.y) * Runner.DeltaTime;

            if (stateMachine.curStateStr != "Falling" && stateMachine.curStateStr != "Land" && movement.Kcc.RealVelocity.y <= -movement.JumpForce)
            {

                Debug.Log("falling");
                stateMachine.ChangeState(PlayerState.Falling);
            }


            if (VelocityY <= 3f && movement.IsGround())
            {
                VelocityY = 0f;
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
