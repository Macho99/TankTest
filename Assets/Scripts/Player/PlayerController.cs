using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
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
    public PlayerRigManager rigManager { get; private set; }
    public Inventory Inventory { get; private set; }
    private LocalPlayerDebugUI LocaldebugUI;

    [SerializeField] private PlayerIngameDebugUI IngamedebugUI;
    [Networked] public float VelocityY { get; set; }
    [Networked, OnChangedRender(nameof(OnChangeUpperLayerWeight))] public float UpperLayerWeight { get; set; }
    private void Awake()
    {
        myCollider = GetComponentInChildren<CapsuleCollider>();
        animator = GetComponent<Animator>();
        stateMachine = GetComponent<NetworkStateMachine>();
        movement = GetComponent<PlayerLocomotion>();
        InputListner = GetComponent<PlayerInputListner>();
        interact = GetComponent<PlayerInteract>();
        rigManager = GetComponent<PlayerRigManager>();
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


        IngamedebugUI.SetPlayerType($"{Object.InputAuthority} ({(HasInputAuthority ? "Input Authority" : (HasStateAuthority ? "State Authority" : "Proxy"))})");



    }

    public override void FixedUpdateNetwork()
    {
        if (InputListner.pressButton.IsSet(NetworkInputData.ButtonType.MouseLock))
        {
            if (Cursor.visible == true)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }


      


        Falling();
        movement.Move();


        if (InputListner.pressButton.IsSet(NetworkInputData.ButtonType.DebugText))
        {
            ClearDebugText();
        }

    }
    public override void Render()
    {
        AddDebugText("State", stateMachine.curStateStr);
    }
    public void Falling()
    {
        if (movement.Kcc.RealVelocity.y <= -1F)
        {
            VelocityY += Mathf.Abs(movement.Kcc.RealVelocity.y) * Runner.DeltaTime;

            animator.SetFloat("VelocityY", VelocityY);

            if (stateMachine.curStateStr != "Falling" && stateMachine.curStateStr != "Land" && movement.Kcc.RealVelocity.y <= -movement.JumpForce)
                stateMachine.ChangeState(PlayerState.Falling);


            
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

    public void AddDebugText(string title, string text)
    {
        IngamedebugUI.AddDebugText(title, text);
    }
    public void ClearDebugText()
    {
        IngamedebugUI.AllClearDubugText();

    }
    public void OnChangeUpperLayerWeight()
    {
        animator.SetLayerWeight(1, UpperLayerWeight);
    }
    public void Aiming()
    {
        if (InputListner.pressButton.IsSet(NetworkInputData.ButtonType.Adherence))
        {
            animator.SetBool("Aim", true);
            rigManager.LeftHandweight = 1f;
            movement.CamController.ChangeCamera(BasicCamController.CameraType.Aim);
        }
        else if (InputListner.releaseButton.IsSet(NetworkInputData.ButtonType.Adherence))
        {
            animator.SetBool("Aim", false);
            rigManager.LeftHandweight = 0f;
            movement.CamController.ChangeCamera(BasicCamController.CameraType.None);
        }
    }
}
