using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class TankMoveState : NetworkBaseState
{
	protected TankMove owner;

	public TankMoveState(TankMove owner)
	{
		this.owner = owner;
	}
}