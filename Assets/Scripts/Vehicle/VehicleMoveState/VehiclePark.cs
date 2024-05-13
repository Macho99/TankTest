using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class VehiclePark : VehicleMoveState
{
	public VehiclePark(VehicleMove owner) : base(owner)
	{
	}

	public override void Enter()
	{
		owner.Reverse = false;
		owner.SetDashBoardGear("P");
		owner.BrakeMultiplier = 1f;
	}

	public override void Exit()
	{
		owner.BrakeMultiplier = 0f;
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		float y = owner.MoveInput.y;
		if(y < -0.1f)
		{
			if (owner.Velocity < 3f)
			{
				owner.Reverse = true;
				ChangeState(VehicleMove.State.ReverseRpmMatch);
			}
		}
		else if (y > 0.1f && owner.Velocity > -3f)
		//else if (owner.MoveInput.sqrMagnitude > 0.1f && owner.Velocity > -3f)
		{
			ChangeState(VehicleMove.State.RpmMatch);
		}
	}

	public override void FixedUpdateNetwork()
	{

	}
}