using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PlayerStatType { HPGauge, ThirstGauge, HungerGauge, PoisoningGauge, Size }

public class PlayerStat : NetworkBehaviour, IHittable, IAfterSpawned
{
    [Networked, Capacity((int)PlayerStatType.Size), OnChangedRender(nameof(UpdateStat))] public NetworkArray<PlayerStatData> statData { get; }

    private Animator animator;

    private PlayerMainUI mainUI;
    public long HitID { get { return Object.Id.Raw << 32; } }
    private NetworkStateMachine stateMachine;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        stateMachine = GetComponent<NetworkStateMachine>();
    }
    public override void Spawned()
    {


    }
    public override void Render()
    {

    }
    public override void FixedUpdateNetwork()
    {
        //if (GetInput(out NetworkInputData input))
        //{
        //    if (input.buttons.IsSet(ButtonType.Attack))
        //    {
        //        PlayerStatData newData = statData[(int)PlayerStatType.HPGauge];
        //        newData.currentValue -= 1;
        //        statData.Set((int)PlayerStatType.HPGauge, newData);
        //        mainUI?.UpdateStat(PlayerStatType.HPGauge, newData.currentValue, newData.maxValue);
        //    }
        //}


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
    private void UpdateStat()
    {
        if (HasInputAuthority)
        {

        }
    }


    public void ApplyDamage(Transform source, Vector3 point, Vector3 force, int damage)
    {
        force.y = 0;

        float angle = Vector3.Angle(transform.forward, force);
        // 포워드와 force가 0일때 같은방향  x = 0 ,y =1
        //포워드와 force가 -90일떄 왼쪽방향 x = -1, y = 0;

        Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);

        Vector3 rot = quat * transform.forward;

       print(damage);

        //Quaternion.LookRotation(transform.forward, .normalized);
        animator.SetFloat("BeshotDirX", rot.x);
        animator.SetFloat("BeshotDirZ", rot.z);



        PlayerStatData newStatData = statData[(int)PlayerStatType.HPGauge];
        newStatData.currentValue -= damage;
        if (newStatData.currentValue < 0)
            newStatData.currentValue = 0;

        if (newStatData.currentValue <= 0)
        {
            //죽음
        }
        else
        {
            stateMachine.ChangeState(PlayerController.PlayerState.Hit);
        }

        statData.Set((int)PlayerStatType.HPGauge, newStatData);
        mainUI?.UpdateStat(PlayerStatType.HPGauge, statData[(int)PlayerStatType.HPGauge].currentValue, statData[(int)PlayerStatType.HPGauge].maxValue);
    }

    public void AfterSpawned()
    {
        if (HasStateAuthority)
        {
            mainUI = GetComponent<PlayerController>().mainUI;
            for (int i = 0; i < statData.Length; i++)
            {
                if (i != (int)PlayerStatType.PoisoningGauge)
                    statData.Set(i, new PlayerStatData(100, 100));
                else
                    statData.Set(i, new PlayerStatData(0, 100));
            }
        }


        if (HasInputAuthority)
        {
            for (int i = 0; i < statData.Length; i++)
            {
                mainUI?.UpdateStat((PlayerStatType)i, statData[i].currentValue, statData[i].maxValue);
            }
        }
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