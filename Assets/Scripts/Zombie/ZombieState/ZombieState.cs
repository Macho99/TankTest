using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ZombieState : NetworkBaseState
{
	protected Zombie owner;

	public ZombieState(Zombie owner)
	{
		this.owner = owner;
	}
}