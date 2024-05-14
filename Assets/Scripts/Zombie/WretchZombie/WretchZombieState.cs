using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class WretchZombieState : NetworkBaseState
{
	protected WretchZombie owner;

	public WretchZombieState(WretchZombie owner)
	{
		this.owner = owner;
	}
}