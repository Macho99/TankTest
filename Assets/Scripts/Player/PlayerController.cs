using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public enum PlayerState { StandLocomotion, CrouchLocomotion, Jump, Land, Falling, ClimbPass, Interact }

    public GameObject debugUIPrefab;
    public NetworkStateMachine stateMachine { get; private set; }
    public Animator animator { get; private set; }
    public PlayerLocomotion movement { get; private set; }
    public PlayerInteract interact { get; private set; }
    private CapsuleCollider myCollider;
    public PlayerInputListner InputListner { get; private set; }
    private HostClientDebugUI debugUI;
    [Networked] public float FallingTime { get; set; }

    private void Awake()
    {
        myCollider = GetComponentInChildren<CapsuleCollider>();
        animator = GetComponent<Animator>();
        stateMachine = GetComponent<NetworkStateMachine>();
        movement = GetComponent<PlayerLocomotion>();
        InputListner = GetComponent<PlayerInputListner>();
        interact = GetComponent<PlayerInteract>();
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

        if (Object.InputAuthority == Runner.LocalPlayer)
        {
            debugUI = GameManager.UI.ShowSceneUI<HostClientDebugUI>("UI/PlayerUI/DebugUI");
            debugUI.SetPlayerInfo(HasStateAuthority == true ? "Host" : "Client");

        }
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

        if(InputListner.pressButton.IsSet(NetworkInputData.ButtonType.DebugText))
        {
            ClearDebugText();
        }

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
    public bool RaycastObject(out RaycastHit hitInfo)
    {
        Ray ray = new Ray();

        ray.origin = transform.position + transform.forward * movement.Kcc.Settings.Radius + Vector3.up * 0.3f;
        ray.direction = transform.forward;

        if (Physics.Raycast(ray, out RaycastHit hit, 1f))
        {
            if (hit.collider.TryGetComponent(out IClimbPassable climbObject))
            {
                if (climbObject.CanClimbPassCheck(hit, transform.position, movement.Kcc.Settings.Height))
                {
                    hitInfo = hit;
                    return true;
                }
            }
        }
        Debug.DrawRay(ray.origin, ray.direction * 1f, Color.red, 0.5f);
        hitInfo = default;
        return false;
    }
    public void AddDebugText(string text)
    {
        if (HasInputAuthority)
            debugUI.AddDebugText(text);
    }
    public void ClearDebugText()
    {
        if (HasInputAuthority)
            debugUI.ClearAllText();

    }

}
