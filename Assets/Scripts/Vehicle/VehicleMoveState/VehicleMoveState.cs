using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class VehicleMoveState : NetworkBaseState
{
	protected TankMove owner;

	public VehicleMoveState(TankMove owner)
	{
		this.owner = owner;
	}
}