using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
	public string curStateStr;
	private Dictionary<string, BaseState> stateDic = new Dictionary<string, BaseState>();
	private BaseState curState;

	public void Start()
	{
		curState.Enter();
	}

	public void Update()
	{
		curState.Update();
		curState.Transition();
	}

	public void FixedUpdate()
	{
		curState.FixedUpdate();
	}

	public void LateUpdate()
	{
		curState.LateUpdate();
	}

	public void InitState(string stateName)
	{
		curStateStr = stateName;
		curState = stateDic[stateName];
	}

	public void AddState(string stateName, BaseState state)
	{
		state.SetStateMachine(this);
		stateDic.Add(stateName, state);
	}

	public void ChangeState(string stateName)
	{
		curState.Exit();
		StopAllCoroutines();
		curStateStr = stateName;
		curState = stateDic[stateName];
		curState.Enter();
	}

	public void InitState<T>(T stateType) where T : Enum
	{
		InitState(stateType.ToString());
	}

	public void AddState<T>(T stateType, BaseState state) where T : Enum
	{
		AddState(stateType.ToString(), state);
	}

	public void ChangeState<T>(T stateType) where T : Enum
	{
		ChangeState(stateType.ToString());
	}
}
