using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
	public enum State { Idle, Steer, Trace, }
	[SerializeField] Transform target;
	[SerializeField] bool follow;

	NavMeshAgent agent;
	StateMachine stateMachine;
	Animator anim;

	public Transform Target { get { return target; } }
	public bool Follow { get { return follow; } }
	public bool HasPath { get { return agent.hasPath; } }
	public Vector3 SteeringTarget { get { return agent.steeringTarget; } }
	public Vector3 DesiredVelocity { get { return agent.desiredVelocity; } }


	private void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();

		stateMachine = gameObject.AddComponent<StateMachine>();
		stateMachine.AddState(State.Idle, new ZombieIdle(this));
		stateMachine.AddState(State.Trace, new ZombieTrace(this));

		stateMachine.InitState(State.Idle);
	}
}