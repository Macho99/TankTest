using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BruteZombie : ZombieBase
{
	[SerializeField] Transform lookTarget;
	[SerializeField] float lookSpeed = 1.5f;

	public enum State { Idle, Trace, Search, }

	[Networked] public Vector3 LookPos {  get; set; }

	protected override void Awake()
	{
		base.Awake();

		stateMachine.AddState(State.Idle, new BruteIdle(this));
		stateMachine.AddState(State.Trace, new BruteTrace(this));
		stateMachine.AddState(State.Search, new BruteSearch(this));

		stateMachine.InitState(State.Idle);
	}

	public override void FixedUpdateNetwork()
	{
		base.FixedUpdateNetwork();

		if(Target != null)
		{
			LookPos = Target.position;
		}
		else
		{
			LookPos = transform.TransformPoint(Vector3.forward * 5f);
		}
	}

	public override void Render()
	{
		base.Render();

		lookTarget.position = Vector3.Lerp(lookTarget.position, LookPos, Runner.DeltaTime * lookSpeed);
	}

	public override string DecideState()
	{
		throw new NotImplementedException();
	}
}