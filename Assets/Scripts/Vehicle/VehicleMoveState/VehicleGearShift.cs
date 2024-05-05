using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class VehicleGearShift : VehicleMoveState
{
	float rpm;
	float velocity;

	public VehicleGearShift(VehicleMove owner) : base(owner)
	{
	}

	public override void Enter()
	{
		velocity = 0f;
		owner.TorqueMultiplier = 0f;
		rpm = owner.EngineRpm;
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

	}

	public override void FixedUpdateNetwork()
	{
		float targetRpm = owner.CurGearRatio * owner.AbsWheelRpm;
		// TODO : 스무스 댐프 바꾸기
		rpm = Mathf.SmoothDamp(rpm, targetRpm, ref velocity, 0.05f);

		owner.SetEngineRpmWithoutWheel(rpm);

		if(targetRpm < owner.MinEngineRpm)
		{
			if(owner.CurGear > 0)
			{
				owner.CurGear--;
				ChangeState(TankMove.State.GearShift);
			}
			else
			{
				ChangeState(TankMove.State.Park);
			}
		}

		if(Mathf.Abs(targetRpm - rpm) < 50f)
		{
			ChangeState(TankMove.State.Drive);
		}
	}
}