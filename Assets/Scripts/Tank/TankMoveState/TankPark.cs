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
		owner.BrakeMultiplier = 1f;
	}

	public override void Exit()
	{
		owner.BrakeMultiplier = 0f;
	}

	public override void Transition()
	{
		if(owner.RawMoveInput.sqrMagnitude > 0.1f)
		{
			ChangeState(TankMove.State.RPMMatch);
		}
	}

	public override void Update()
	{

	}
}