using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Zombie : NetworkBehaviour
{
	public enum State { Idle, Wander, Trace, AnimWait, CrawlIdle, }
	[SerializeField] float minIdleTime = 1f;
	[SerializeField] float maxIdleTime = 10f;
	[SerializeField] Transform skins;
	[SerializeField] float fallAsleepThreshold = 0.2f;
	[SerializeField] TextMeshProUGUI curStateText;

	NavMeshAgent agent;
	NetworkStateMachine stateMachine;
	Animator anim;

	public float FallAsleepThreshold { get { return fallAsleepThreshold; } }
	public NavMeshAgent Agent { get { return agent; } }
	public bool Aggresive { get; set; }
	public bool HasPath { get { return agent.hasPath; } }
	public float RemainDist { get { return agent.remainingDistance; } }
	public Vector3 SteeringTarget { get { return agent.steeringTarget; } }
	public Vector3 DesiredDir { get { return agent.desiredVelocity.normalized; } }
	public LayerMask FallAsleepMask { get; set; }
	//	#region Variable For Specific State
	// Idle State
	public float MinIdleTime { get { return minIdleTime; } }
	public float MaxIdleTime { get { return maxIdleTime; } }

	// AnimWait State
	public AnimWaitStruct? AnimWaitStruct { get; set; }
	//	#endregion

	public NetworkObject Target { get; private set; }
	public float TraceSpeed { get; private set; }

	[Networked] public int SkinIdx { get; set; }
	[Networked] public Vector3 Position { get; set; }
	[Networked] public Quaternion Rotation { get; set; }
	[Networked] public Vector3 RagdollVelocity { get; set; }
	[Networked] public int CurHP { get; set; }

	private void Awake()
	{
		FallAsleepMask = LayerMask.GetMask("FallAsleepObject");
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
		stateMachine = GetComponent<NetworkStateMachine>();

		stateMachine.AddState(State.Idle, new ZombieIdle(this));
		stateMachine.AddState(State.Trace, new ZombieTrace(this));
		stateMachine.AddState(State.Wander, new ZombieWander(this));
		stateMachine.AddState(State.AnimWait, new ZombieAnimWait(this));
		stateMachine.AddState(State.CrawlIdle, new ZombieCrawlIdle(this));

		stateMachine.InitState(State.Idle);

		Rigidbody[] bodys = GetComponentsInChildren<Rigidbody>();
		foreach(Rigidbody body in bodys)
		{
			body.isKinematic = true;
			body.detectCollisions = false;
			Collider col = body.GetComponent<Collider>();
			col.isTrigger = true;
		}
	}

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(1f);

		anim.enabled = false;
		Agent.enabled = false;
		Rigidbody[] bodys = GetComponentsInChildren<Rigidbody>();
		bool pushed = false;
		foreach (Rigidbody body in bodys)
		{
			body.isKinematic = false;
			body.detectCollisions = true;
			Collider col = body.GetComponent<Collider>();
			col.isTrigger = false;
			if(pushed == false)
			{
				body.AddForce((-transform.forward + transform.up * 0.3f) * 400f, ForceMode.Impulse);
				pushed = true;
			}
		}
	}

	public override void Spawned()
	{
		Agent.enabled = true;
		int cnt = 0;
		foreach (Transform child in skins)
		{
			if (cnt == SkinIdx)
				child.gameObject.SetActive(true);
			else
				child.gameObject.SetActive(false);
			cnt++;
		}
	}

	public override void FixedUpdateNetwork()
	{
		Position = transform.position;
		Rotation = transform.rotation;
	}

	public override void Render()
	{
		StringBuilder sb = new StringBuilder();
		//sb.AppendLine($"현재 상태: {stateMachine.curStateStr}");
		sb.AppendLine($"SpeedX : {anim.GetFloat("SpeedX"):#.##}");
		sb.AppendLine($"SpeedY : {anim.GetFloat("SpeedY"):#.##}");
		sb.AppendLine($"curPos : {transform.position}");
		sb.AppendLine($"Pos: {Position}");

		curStateText.text = sb.ToString();
		if (Object.IsProxy)
		{
			if((transform.position - Position).sqrMagnitude > 1f)
			{
				//debugCapsule.transform.position = transform.position;
				//debugCapsule.SetActive(true);
				Agent.enabled = false;
				transform.position = Position;
				Agent.enabled = true;
			}
			transform.rotation = Rotation;
		}
	}

	public void Init(NetworkObject target)
	{
		Target = target;
		TraceSpeed = Random.Range(1, 4);

		int skinCnt = skins.childCount;
		SkinIdx = Random.Range(0, skinCnt);
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
			deltaTime = Runner.DeltaTime;
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