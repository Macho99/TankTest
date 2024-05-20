using Fusion;
using Fusion.Addons.FSM;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : NetworkBehaviour, IAfterSpawned, IStateMachineOwner
{
    public enum PlayerState { StandLocomotion, CrouchLocomotion, Jump, Land, Falling, Hit, Dead }
    [SerializeField] private Transform presetRoot;
    [SerializeField] private Transform hairRoot;
    [SerializeField] private Transform breardRoot;
    [SerializeField] private PlayerStates[] states;
    [Networked, Capacity((int)AppearanceType.Size)] public NetworkArray<int> decorations { get; }

    public StateMachine<PlayerStates> stateMachine { get; private set; }
    public Animator animator { get; private set; }
    public PlayerLocomotion movement { get; private set; }
    public PlayerInteract interact { get; private set; }
    private CapsuleCollider myCollider;
    public PlayerInputListner InputListner { get; private set; }
    public Inventory Inventory { get; private set; }

    public WeaponController weaponController { get; private set; }
    private LocalPlayerDebugUI LocaldebugUI;
    public PlayerMainUI mainUI { get; private set; }
    [Networked] public float VelocityY { get; set; }
    public PlayerStates GetState(PlayerState playerState)
    {
        return states[(int)playerState];
    }

    private void Awake()
    {
        myCollider = GetComponentInChildren<CapsuleCollider>();
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerLocomotion>();
        InputListner = GetComponent<PlayerInputListner>();
        interact = GetComponent<PlayerInteract>();
        weaponController = GetComponent<WeaponController>();
        Inventory = GetComponentInChildren<Inventory>();
        //stateMachine.AddState(PlayerState.StandLocomotion, new PlayerStandLocomotionState(this));
        //stateMachine.AddState(PlayerState.CrouchLocomotion, new PlayerCrouchLocomotionState(this));
        //stateMachine.AddState(PlayerState.Jump, new PlayerJumpState(this));
        //stateMachine.AddState(PlayerState.Land, new PlayerLandState(this));
        //stateMachine.AddState(PlayerState.Falling, new PlayerFallingState(this));
        //stateMachine.AddState(PlayerState.Interact, new PlayerInteractState(this));
        //stateMachine.AddState(PlayerState.Hit, new PlayerHitState(this));
        //stateMachine.AddState(PlayerState.Dead, new PlayerDeadState(this));

        //stateMachine.InitState(PlayerState.StandLocomotion);


    }
    public void SetupDecoration(AppearanceType type, int index)
    {
        decorations.Set((int)type, index);
    }
    public override void Spawned()
    {
        VelocityY = 0f;
        name = $"{Object.InputAuthority} ({(HasInputAuthority ? "Input Authority" : (HasStateAuthority ? "State Authority" : "Proxy"))})";
        if (HasInputAuthority)
        {
            mainUI = GameManager.UI.ShowSceneUI<PlayerMainUI>("UI/PlayerUI/PlayerMainUI");
        }

        hairRoot.GetChild(decorations[(int)AppearanceType.Hair]).gameObject.SetActive(true);
        breardRoot.GetChild(decorations[(int)AppearanceType.Breard]).gameObject.SetActive(true);
        presetRoot.GetChild(decorations[(int)AppearanceType.Preset]).gameObject.SetActive(true);


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

            if (stateMachine.ActiveStateId != (int)PlayerState.Falling && stateMachine.ActiveStateId != (int)PlayerState.Land && movement.Kcc.RealVelocity.y <= -movement.JumpForce)
            {

                Debug.Log("falling");
                stateMachine.TryActivateState((int)PlayerState.Falling);
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

    public void AfterSpawned()
    {

    }

    public void CollectStateMachines(List<IStateMachine> stateMachines)
    {
        name = $"{Object.InputAuthority} ({(HasInputAuthority ? "Input Authority" : (HasStateAuthority ? "State Authority" : "Proxy"))})";
        stateMachine = new StateMachine<PlayerStates>("Player", states);
        stateMachines.Add(this.stateMachine);
    }
}
