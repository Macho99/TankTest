using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class BruteZombieState : NetworkBaseState
{
	protected BruteZombie owner;

	public BruteZombieState(BruteZombie owner)
	{
		this.owner = owner;
	}
}