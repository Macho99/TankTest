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
		rpm = owner.MinEngineRpm;
	}

	public override void Exit()
	{

	}

	public override void Transition()
	{
		//owner.WheelRpm
	}

	public override void Update()
	{
		//if ()
		{
			rpm += Time.deltaTime * owner.RpmMatchSpeed;
		}
		owner.SetEngineRpmWithoutWheel(rpm);
	}
}
