using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TankPark : VehiclePark
{
	public TankPark(VehicleMove owner) : base(owner)
	{
	}

	public override void Transition()
	{
		//base.Transition();
		float y = owner.MoveInput.y;
		if (y < -0.1f)
		{
			if (owner.Velocity < 3f)
			{
				owner.Reverse = true;
				ChangeState(VehicleMove.State.ReverseRpmMatch);
			}
		}
		else if (owner.MoveInput.sqrMagnitude > 0.1f && owner.Velocity > -3f)
		{
			ChangeState(VehicleMove.State.RpmMatch);
		}
	}
}