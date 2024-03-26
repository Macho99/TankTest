using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState
{
	private StateMachine stateMachine;

	public void SetStateMachine(StateMachine stateMachine)
	{
		this.stateMachine = stateMachine;
		SetUp();
	}

	protected void ChangeState(string stateName)
	{
		stateMachine.ChangeState(stateName);
	}

	protected void ChangeState<T>(T stateType) where T : Enum
	{
		ChangeState(stateType.ToString());
	}

	public abstract void SetUp();
	public abstract void Enter();
	public abstract void Exit();
	public abstract void Update();
	public abstract void Transition();
	public virtual void LateUpdate() { }
	public virtual void FixedUpdate() { }
}