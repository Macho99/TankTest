using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TankRpmMatch : TankMoveState
{
	float rpm;

	public TankRpmMatch(TankMove owner) : base(owner)
	{
	}

	public override void Enter()
	{
		owner.SetDashBoardGear((owner.CurGear + 1).ToString());
		owner.TorqueMultiplier = 1f;
		rpm = owner.MinEngineRpm;
	}

	public override void Exit()
	{
		owner.TorqueMultiplier = 1f;
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		if(owner.Direction == 1 && owner.WheelRpm * owner.CurGearRatio > rpm)
		{
			ChangeState(TankMove.State.Drive);
			return;
		}

		if(rpm < owner.MinEngineRpm)
		{
			ChangeState(TankMove.State.Park);
		}
	}

	public override void Update()
	{
		if(owner.RawMoveInput.sqrMagnitude > 0.1f)
		{
			if (rpm < owner.MaxTorqueRpm)
			{
				owner.TorqueMultiplier = 1f;
				rpm += Time.deltaTime * owner.RpmMatchSpeed;
				owner.SetEngineRpmWithoutWheel(rpm);
			}
			//if(owner.)
		}
		else if(owner.RawMoveInput.y < 0.1f)
		{
			owner.TorqueMultiplier = 1f;
			rpm -= Time.deltaTime * owner.RpmMatchSpeed * 2f;
			owner.SetEngineRpmWithoutWheel(rpm);
		}
	}
}
