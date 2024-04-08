using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ZombieState : BaseState
{
	protected Zombie owner;
	protected PhotonView photonView;

	public ZombieState(Zombie owner)
	{
		this.owner = owner;
		this.photonView = owner.photonView;
	}
}