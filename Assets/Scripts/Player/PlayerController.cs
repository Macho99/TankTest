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
    [Networked] public float FallingTime { get; set; }

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
        FallingTime = 0f;
        name = $"{Object.InputAuthority} ({(HasInputAuthority ? "Input Authority" : (HasStateAuthority ? "State Authority" : "Proxy"))})";


        IngamedebugUI.SetPlayerType($"{Object.InputAuthority} ({(HasInputAuthority ? "Input Authority" : (HasStateAuthority ? "State Authority" : "Proxy"))})");


        //debugUI = GameManager.UI.ShowInGameUI<LocalPlayerDebugUI>("UI/PlayerUI/DebugUI");
        //debugUI.SetPlayerInfo(HasStateAuthority == true ? "Host" : "Client");
        //debugUI.SetTarget(this.transform);
        //debugUI.SetOffset(new Vector3(0f, 800f, 0f));


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


        if (!movement.IsGround() && movement.Kcc.RealVelocity.y < 0f)
        {
            FallingTime += Runner.DeltaTime;

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

    public void AddDebugText(string title, string text)
    {
        IngamedebugUI.AddDebugText(title, text);
    }
    public void ClearDebugText()
    {
        IngamedebugUI.AllClearDubugText();

    }

}
