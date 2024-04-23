using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BruteDefenceKnockback : BruteZombieState
{
	const float speed = 1f;
	const float rotateSpeed = 60f;
	const float defenceTime = 5f;

	const string animName = "Hit";

	bool animEntered;

	public BruteDefenceKnockback(BruteZombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		animEntered = false;
		owner.Shield.SetEnable(true);
		owner.OnStun += StunCallback;
	}

	public override void Exit()
	{
		owner.Shield.SetEnable(false);
		owner.OnStun -= StunCallback;
	}

	private void StunCallback()
	{
		owner.SetAnimTrigger("DefenceExit");
	}

	public override void FixedUpdateNetwork()
	{
		owner.LookTarget();
		owner.Decelerate();

		if (animEntered == true) { return; }
		if (owner.IsAnimName(animName) == true)
		{
			animEntered = true;
		}
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		if (animEntered == true)
		{
			if (owner.IsAnimName(animName) == false)
			{
				ChangeState(BruteZombie.State.DefenceTrace);
			}
		}
	}
}