using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class VehicleReverseRpmMatch : VehicleMoveState
{
	float rpm;

	public VehicleReverseRpmMatch(TankMove owner) : base(owner)
	{
	}
	public override void Enter()
	{
		owner.SetDashBoardGear("R");
		rpm = owner.MinEngineRpm;
	}

	public override void Exit()
	{
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		if (owner.Direction == -1 && owner.AbsWheelRpm * owner.CurGearRatio > rpm)
		{
			ChangeState(TankMove.State.Reverse);
			return;
		}

		if (rpm < owner.MinEngineRpm)
		{
			ChangeState(TankMove.State.Park);
		}
	}

	public override void FixedUpdateNetwork()
	{
		Vector2 moveInput = owner.MoveInput;
		//후진 입력 없으면 rpm 낮추기
		if (moveInput.y > -0.1f)// && Mathf.Approximately(moveInput.x, 0f) == true)
		{
			owner.TorqueMultiplier = 0f;
			rpm -= owner.Runner.DeltaTime * owner.RpmMatchSpeed * 3f;
			owner.SetEngineRpmWithoutWheel(rpm);
		}
		else
		{
			if (moveInput.y < -0.1f)
			{
				owner.TorqueMultiplier = -1f;
			}
			else
			{
				owner.TorqueMultiplier = 0f;
			}

			if (rpm < owner.MaxTorqueRpm)
			{
				rpm += owner.Runner.DeltaTime * owner.RpmMatchSpeed;
				owner.SetEngineRpmWithoutWheel(rpm);
			}
		}
	}
}