using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class StateMachine<TState, TOwner>
{
	private TOwner owner;
	private TState curStateEnum;
	private Dictionary<TState, StateBase<TState, TOwner>> states;
	private StateBase<TState, TOwner> curState;
	public StateMachine(TOwner owner)
	{
		this.owner = owner;
		this.states = new Dictionary<TState, StateBase<TState, TOwner>>();
	}

	public void AddState(TState state, StateBase<TState, TOwner> stateBase)
	{
		states.Add(state, stateBase);
	}

	public void SetUp(TState startState)
	{
		foreach (StateBase<TState, TOwner> state in states.Values)
		{
			state.Setup();
		}

		curStateEnum = startState;
		curState = states[startState];
		curState.Enter();
	}

	public void Update()
	{
		curState?.Update();
		curState?.Transition();
	}

	public void ChangeState(TState newState)
	{
		//Debug.Log(newState);
		curState?.Exit();
		curStateEnum = newState;
		curState = states[newState];
		curState.Enter();
	}

	public TState GetCurState()
	{
		return curStateEnum;
	}
}