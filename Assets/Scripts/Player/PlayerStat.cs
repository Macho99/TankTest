using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
public enum PlayerStatType { HPGauge, ThirstGauge, HungerGauge, PoisoningGauge, Size }

public class PlayerStat : NetworkBehaviour, IHittable
{
    [Networked, Capacity((int)PlayerStatType.Size), OnChangedRender(nameof(UpdateStat))] public NetworkArray<PlayerStatData> statData { get; }
   

    private PlayerMainUI mainUI;
    public long HitID { get; }
    public override void Spawned()
    {

        if (HasInputAuthority)
        {
            mainUI = GameManager.UI.ShowSceneUI<PlayerMainUI>("UI/PlayerUI/PlayerMainUI");
            for (int i = 0; i < statData.Length; i++)
            {
                statData.Set(i, new PlayerStatData(100, 100));
                mainUI.UpdateStat((PlayerStatType)i, statData[i].currentValue, statData[i].maxValue);
            }



        }
    }
    public override void Render()
    {

    }
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            if (input.buttons.IsSet(ButtonType.Attack))
            {
                PlayerStatData newData = statData[(int)PlayerStatType.HPGauge];
                newData.currentValue -= 1;
                statData.Set((int)PlayerStatType.HPGauge, newData);
                mainUI?.UpdateStat(PlayerStatType.HPGauge, newData.currentValue, newData.maxValue);
            }
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
    private void UpdateStat()
    {
        if (HasInputAuthority)
        {

        }
    }

    public void ApplyDamage(Transform source, Vector3 point, Vector3 force, int damage)
    {

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