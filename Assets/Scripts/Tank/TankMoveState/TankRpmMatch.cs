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
		if(owner.Direction == 1 && owner.AbsWheelRpm * owner.CurGearRatio > rpm)
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
		Vector2 moveInput = owner.RawMoveInput;
		//전진 입력 없고, 좌우 입력 없으면 rpm 낮추기
		if(moveInput.y < 0.1f && Mathf.Approximately(moveInput.x, 0f) == true)
		{
			owner.TorqueMultiplier = 0f;
			rpm -= Time.deltaTime * owner.RpmMatchSpeed * 3f;
			owner.SetEngineRpmWithoutWheel(rpm);
		}
		else
		{
			if(moveInput.y > 0.1f)
			{
				owner.TorqueMultiplier = 1f;
			}
			else
			{
				owner.TorqueMultiplier = 0f;
			}

			if (rpm < owner.MaxTorqueRpm)
			{
				rpm += Time.deltaTime * owner.RpmMatchSpeed;
				owner.SetEngineRpmWithoutWheel(rpm);
			}
		}
	}
}
