using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TankPark : TankMoveState
{
	public TankPark(TankMove owner) : base(owner)
	{
	}

	public override void Enter()
	{
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
		float y = owner.RawMoveInput.y;
		if(y < -0.1f)
		{

		}
		else if (owner.RawMoveInput.sqrMagnitude > 0.1f)
		{
			ChangeState(TankMove.State.RPMMatch);
		}
	}

	public override void Update()
	{

	}
}