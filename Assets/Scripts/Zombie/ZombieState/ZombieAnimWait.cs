using System.Collections.Generic;
public class ZombieAnimWait : ZombieState
{
	string animName;
	Zombie.State nextState;
	bool animEntered;

	public ZombieAnimWait(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		animEntered = false;
		animName = owner.AnimNameToWaitEnd;
		nextState = owner.AfterAnimEndState;
	}

	public override void Exit()
	{
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		if (animEntered == true)
		{
			if(owner.IsAnimName(animName) == false)
			{
				ChangeState(nextState);
			}
		}
	}

	public override void Update()
	{
		if(animEntered == true) { return; }


		if(owner.IsAnimName(animName) == true)
		{
			animEntered = true;
			owner.AnimStartAction?.Invoke();
			owner.AnimStartAction = null;
		}
	}
}