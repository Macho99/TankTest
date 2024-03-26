using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TankGearShift : TankMoveState
{
	float rpm;
	float startRpm;
	float elapsed;
	public TankGearShift(TankMove owner) : base(owner)
	{
	}

	public override void Enter()
	{
		elapsed = 0f;
		owner.TorqueMultiplier = 1f;
		rpm = owner.EngineRpm;
		startRpm = rpm;
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

	public override void Update()
	{
		elapsed += Time.deltaTime;
		float targetRpm = owner.CurGearRatio * owner.WheelRpm;
		rpm = Mathf.Lerp(startRpm, targetRpm, elapsed / 0.1f);

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