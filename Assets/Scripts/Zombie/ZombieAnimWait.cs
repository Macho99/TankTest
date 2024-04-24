using System;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAnimWait : NetworkBaseState
{
	ZombieBase owner;
	AnimWaitStruct waitStruct;
	bool lastAnimEntered;
	bool startAnimEntered;

	public ZombieAnimWait(ZombieBase owner)
	{
		this.owner = owner;
	}

	public override void Enter()
	{
		if (owner.AnimWaitStruct.HasValue == false)
		{
			Debug.LogError("AnimWaitStruct를 설정하세요");
			return;
		}

		startAnimEntered = false;
		lastAnimEntered = false;
		waitStruct = owner.AnimWaitStruct.Value;
		waitStruct.startAction?.Invoke();
		owner.AnimWaitStruct = null;

		//Debug
		owner.WaitName = waitStruct.animName;
		owner.NextState = waitStruct.nextState;
	}

	public override void Exit()
	{
		waitStruct.exitAction?.Invoke();
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
	}

	public override void FixedUpdateNetwork()
	{
		if (lastAnimEntered == true)
		{
			if (owner.IsAnimName(waitStruct.animName, waitStruct.layer) == false)
			{
				if (waitStruct.nextState == null)
				{
					ChangeState(owner.DecideState());
					return;
				}
				else
				{
					ChangeState(waitStruct.nextState);
					return;
				}
			}
		}

		waitStruct.updateAction?.Invoke();
		if(lastAnimEntered == true) { return; }

		if(startAnimEntered == false)
		{
			if(owner.IsAnimName(waitStruct.startAnimName, waitStruct.layer) == true)
			{
				startAnimEntered = true;
			}
		}

		if(startAnimEntered == true && owner.IsAnimName(waitStruct.animName, waitStruct.layer) == true)
		{
			lastAnimEntered = true;
			waitStruct.animStartAction?.Invoke();
		}
	}
}