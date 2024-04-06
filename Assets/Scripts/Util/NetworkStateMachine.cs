using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NetworkStateMachine : NetworkBehaviour
{
	public string curStateStr;
	private Dictionary<string, NetworkBaseState> stateDic = new Dictionary<string, NetworkBaseState>();
	private NetworkBaseState curState;

	public override void Spawned()
	{
		curState.Enter();
	}

	public override void FixedUpdateNetwork()
	{
		curState.FixedUpdateNetwork();
		curState.Transition();
	}

	public override void Render()
	{
		curState.Render();
	}

	public void InitState(string stateName)
	{
		curStateStr = stateName;
		curState = stateDic[stateName];
	}

	public void AddState(string stateName, NetworkBaseState state)
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

	public void AddState<T>(T stateType, NetworkBaseState state) where T : Enum
	{
		AddState(stateType.ToString(), state);
	}

	public void ChangeState<T>(T stateType) where T : Enum
	{
		ChangeState(stateType.ToString());
	}
}
