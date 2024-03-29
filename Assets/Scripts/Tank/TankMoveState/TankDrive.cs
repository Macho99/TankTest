﻿using System.Collections;
using UnityEngine;

public class TankDrive : TankMoveState
{
	public TankDrive(TankMove owner) : base(owner)
	{
	}

	public override void Enter()
	{
		owner.SetDashBoardGear((owner.CurGear + 1).ToString());
	}

	public override void Exit()
	{
		owner.SetDashBoardGear((owner.CurGear + 1).ToString());
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		if (owner.CurGear < owner.GearChangeSpeeds.Length)
		{
			if (owner.Velocity > owner.GearChangeSpeeds[owner.CurGear])
			{
				owner.CurGear++;
				ChangeState(TankMove.State.GearShift);
				return;
			}
		}

		if (owner.CurGear > 0)
		{
			if (owner.Velocity < owner.GearChangeSpeeds[owner.CurGear - 1] - 4f)
			{
				owner.CurGear--;
				ChangeState(TankMove.State.GearShift);
				return;
			}
		}

		if(owner.EngineRpm < owner.MinEngineRpm)
		{
			owner.CurGear = 0;
			owner.EngineRpm = owner.MinEngineRpm;
			ChangeState(TankMove.State.Park);
			return;
		}
	}

	public override void Update()
	{
		float y = owner.RawMoveInput.y;
		owner.SetEngineRpmWithWheel();
		if(y > -0.1f)
		{
			if (owner.Velocity > owner.MaxSpeed)
			{
				if (owner.Velocity > owner.MaxSpeed + 3f)
				{
					owner.TorqueMultiplier = 0f;
					owner.BrakeMultiplier = 0.3f;
				}
				else
				{
					owner.TorqueMultiplier = 0f;
					owner.BrakeMultiplier = 0f;
				}
			}
			else if(owner.RawMoveInput.sqrMagnitude < 0.1f)
			{
				owner.TorqueMultiplier = 0f;
				owner.BrakeMultiplier = 0.1f;
			}
			else if (y > 0.1f)
			{
				owner.TorqueMultiplier = 1f;
				owner.BrakeMultiplier = 0f;
			}
			else
			{
				owner.TorqueMultiplier = 0f;
				owner.BrakeMultiplier = 0f;
			}
		}
		else if(y < -0.1f)
		{
			owner.TorqueMultiplier = 0f;
			owner.BrakeMultiplier = 1f;
		}
	}
}