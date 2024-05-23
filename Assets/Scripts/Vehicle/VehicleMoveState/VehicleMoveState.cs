using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class VehicleMoveState : NetworkBaseState
{
	protected VehicleMove owner;

	public VehicleMoveState(VehicleMove owner)
	{
		this.owner = owner;
	}
}