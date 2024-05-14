using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ZombieBaseState : NetworkBaseState
{
	protected ZombieBase owner;
	public ZombieBaseState(ZombieBase owner)
	{ 
		this.owner = owner;
	}
}
