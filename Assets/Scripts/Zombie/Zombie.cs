using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;
using Random = UnityEngine.Random;

public class Zombie : MonoBehaviour
{
	public enum State { Idle, Wander, Trace, AnimWait, CrawlIdle, }
	[SerializeField] Transform target;
	[Range(1, 3)]
	[SerializeField] float traceSpeed;
	[SerializeField] float minIdleTime = 1f;
	[SerializeField] float maxIdleTime = 10f;
	[SerializeField] Transform skins;
	[SerializeField] float fallAsleepThreshold = 0.2f;

	NavMeshAgent agent;
	StateMachine stateMachine;
	Animator anim;

	public float FallAsleepThreshold { get { return fallAsleepThreshold; } }
	public NavMeshAgent Agent { get { return agent; } }
	public float TraceSpeed { get { return traceSpeed; } }
	public bool Aggresive { get; set; }
	public Transform Target { get { return target; } }
	public bool HasPath { get { return agent.hasPath; } }
	public float RemainDist { get { return agent.remainingDistance; } }
	public Vector3 SteeringTarget { get { return agent.steeringTarget; } }
	public Vector3 DesiredDir { get { return agent.desiredVelocity.normalized; } }
	#region Variable For Specific State
	// Idle State
	public float MinIdleTime { get { return minIdleTime; } }
	public float MaxIdleTime { get { return maxIdleTime; } }

	// AnimWait State
	public AnimWaitStruct? AnimWaitStruct { get; set; }
	#endregion


	private void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
		stateMachine = GetComponent<StateMachine>();
		if(stateMachine == null)
			stateMachine = gameObject.AddComponent<StateMachine>();

		stateMachine.AddState(State.Idle, new ZombieIdle(this));
		stateMachine.AddState(State.Trace, new ZombieTrace(this));
		stateMachine.AddState(State.Wander, new ZombieWander(this));
		stateMachine.AddState(State.AnimWait, new ZombieAnimWait(this));
		stateMachine.AddState(State.CrawlIdle, new ZombieCrawlIdle(this));

		stateMachine.InitState(State.Idle);
		Init();
	}

	public void Init()
	{
		int skinCnt = skins.childCount;
		int onIdx = Random.Range(0, skinCnt);
		int cnt = 0;
		foreach(Transform child in skins)
		{
			if (cnt == onIdx)
				child.gameObject.SetActive(true);
			else
				child.gameObject.SetActive(false);
			cnt++;
		}
	}
	
	public void SetAnimBool(string name, bool value)
	{
		anim.SetBool(name, value);
	}

	public void SetAnimFloat(string name, float value)
	{
		anim.SetFloat(name, value);
	}

	public void SetAnimFloat(string name, float value, float dampTime, float? deltaTime = null)
	{
		if(deltaTime.HasValue == false)
		{
			deltaTime = Time.deltaTime;
		}
		anim.SetFloat(name, value, dampTime, deltaTime.Value);
	}

	public void SetAnimTrigger(string name)
	{
		anim.SetTrigger(name);
	}

	public bool IsAnimName(string name, int layer = 0)
	{
		return anim.GetCurrentAnimatorStateInfo(layer).IsName(name);
	}
	
	public void SetDestination(Vector3 vec)
	{
		agent.SetDestination(vec);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(transform.position + Vector3.up * 0.3f + transform.forward * 0.1f, 0.05f);
	}

	public State DecisionState()
	{
		if(anim.GetBool("Crawl") == true)
		{
			if(Target == null)
			{
				return State.CrawlIdle;
			}
			else
			{
				return State.CrawlIdle;
				//공격으로 전환
			}
		}
		
		if(Target != null)
		{
			return State.Trace;
		}
		else
		{
			return State.Idle;
		}
	}
}