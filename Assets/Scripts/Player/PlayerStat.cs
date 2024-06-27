using Fusion;
using Fusion.Addons.FSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PlayerStatType { HPGauge, ThirstGauge, HungerGauge, PoisoningGauge, Size }

public class PlayerStat : NetworkBehaviour, IHittable, IAfterSpawned
{
    [Networked, Capacity((int)PlayerStatType.Size)] public NetworkArray<PlayerStatData> statData { get; }

    private Animator animator;

    private PlayerMainUI mainUI;
    public long HitID { get { return Object.Id.Raw << 32; } }
    private StateMachine<PlayerStates> stateMachine;
    private PlayerController controller;

    [Networked]private NetworkBool isHit { get; set; }
    [Networked]private NetworkBool isDead { get; set; }
    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();


    }
    public override void Spawned()
    {


    }
    public override void Render()
    {

    }
    public override void FixedUpdateNetwork()
    {
        if (isHit)
        {
            stateMachine.ForceActivateState((int)PlayerController.PlayerState.Hit);
            isHit = false;
        }
        if(isDead)
        {
            stateMachine.ForceActivateState((int)PlayerController.PlayerState.Dead);
            isDead = false;
        }
    }

    public bool Health(PlayerStatType playerStatType, int Helath)
    {
        PlayerStatData status = statData[(int)playerStatType];
        if (status.currentValue == status.maxValue)
            return false;


        status.currentValue = Mathf.Min(status.currentValue + Helath, status.maxValue);

        statData.Set((int)playerStatType, status);

        mainUI?.UpdateStat(playerStatType, status.currentValue, status.maxValue);
        return true;
    }


    public void ApplyDamage(Transform source, Vector3 point, Vector3 force, int damage)
    {
        if (stateMachine.ActiveStateId == (int)PlayerController.PlayerState.Dead || stateMachine.ActiveStateId == (int)PlayerController.PlayerState.Hit)
        {
            return;
        }
        if (statData[(int)PlayerStatType.HPGauge].currentValue <= 0)
        {
            return;
        }
        if(damage < 6)
		{
			PlayerStatData newStatData2 = statData[(int)PlayerStatType.HPGauge];
			newStatData2.currentValue -= damage;
			if (newStatData2.currentValue < 0)
				newStatData2.currentValue = 0;

			statData.Set((int)PlayerStatType.HPGauge, newStatData2);

			mainUI?.UpdateStat(PlayerStatType.HPGauge, statData[(int)PlayerStatType.HPGauge].currentValue, statData[(int)PlayerStatType.HPGauge].maxValue);

			RPC_ApplyDamage(point, force, damage);
            return;
		}

        force.y = 0;

        float angle = Vector3.Angle(transform.forward, force);
        // 포워드와 force가 0일때 같은방향  x = 0 ,y =1
        //포워드와 force가 -90일떄 왼쪽방향 x = -1, y = 0;

        Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);

        Vector3 rot = quat * transform.forward;


        //Quaternion.LookRotation(transform.forward, .normalized);
        animator.SetFloat("BeshotDirX", rot.x);
        animator.SetFloat("BeshotDirZ", rot.z);



        PlayerStatData newStatData = statData[(int)PlayerStatType.HPGauge];
        newStatData.currentValue -= damage;
        if (newStatData.currentValue < 0)
            newStatData.currentValue = 0;

        statData.Set((int)PlayerStatType.HPGauge, newStatData);

        mainUI?.UpdateStat(PlayerStatType.HPGauge, statData[(int)PlayerStatType.HPGauge].currentValue, statData[(int)PlayerStatType.HPGauge].maxValue);

        if (statData[(int)PlayerStatType.HPGauge].currentValue <= 0)
        {
            isDead = true;
        }
        else
        {
            isHit = true;
        }
        RPC_ApplyDamage(point, force, damage);

    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_ApplyDamage(Vector3 point, Vector3 force, int damage)
    {
      

        mainUI?.UpdateStat(PlayerStatType.HPGauge, statData[(int)PlayerStatType.HPGauge].currentValue, statData[(int)PlayerStatType.HPGauge].maxValue);
    }

    public void AfterSpawned()
    {
        if (HasStateAuthority)
        {

            for (int i = 0; i < statData.Length; i++)
            {
                if (i != (int)PlayerStatType.PoisoningGauge)
                    statData.Set(i, new PlayerStatData(50000, 50000));
                else
                    statData.Set(i, new PlayerStatData(0, 50000));
            }
        }

        if (HasInputAuthority)
        {
            mainUI = GetComponent<PlayerController>().mainUI;

            for (int i = 0; i < statData.Length; i++)
            {
                mainUI.UpdateStat((PlayerStatType)i, statData[i].currentValue, statData[i].maxValue);
            }
        }
        stateMachine = GetComponent<PlayerController>().stateMachine;
    }
}
public struct PlayerStatData : INetworkStruct
{
    public int currentValue;
    public int maxValue;

    public PlayerStatData(int currentValue, int maxValue)
    {
        this.currentValue = currentValue;
        this.maxValue = maxValue;
    }
}