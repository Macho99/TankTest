using System.Collections;
using UnityEngine;

public class TankReverse : TankMoveState
{
	public TankReverse(TankMove owner) : base(owner)
	{
	}

	public override void Enter()
	{
	}

	public override void Exit()
	{
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		if (owner.EngineRpm < owner.MinEngineRpm)
		{
			owner.EngineRpm = owner.MinEngineRpm;
			ChangeState(TankMove.State.Park);
			return;
		}
	}

	public override void Update()
	{
		float y = owner.RawMoveInput.y;
		owner.SetEngineRpmWithWheel();
		if (y < -0.1f)
		{
			if (owner.Velocity < owner.MaxReverseSpeed)
			{
				if (owner.Velocity < owner.MaxReverseSpeed + 3f)
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
			else if (owner.RawMoveInput.sqrMagnitude < 0.1f)
			{
				owner.TorqueMultiplier = 0f;
				owner.BrakeMultiplier = 0.1f;
			}
			else
			{
				owner.TorqueMultiplier = -1f;
				owner.BrakeMultiplier = 0f;
			}
		}
		else
		{
			owner.TorqueMultiplier = 0f;
			owner.BrakeMultiplier = 1f;
		}
	}
}