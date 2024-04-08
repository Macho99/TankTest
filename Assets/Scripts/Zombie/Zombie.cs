using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;
using Random = UnityEngine.Random;

public class Zombie : MonoBehaviourPun
{
	public enum State { Idle, Wander, Trace, AnimWait, Wait, CrawlIdle }
	[SerializeField] float minIdleTime = 1f;
	[SerializeField] float maxIdleTime = 10f;
	[SerializeField] Transform skins;
	[SerializeField] float fallAsleepThreshold = 0.2f;
	[SerializeField] TextMeshProUGUI curStateText;

	NavMeshAgent agent;
	StateMachine stateMachine;
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

	public Transform Target { get; private set; }
	public float MaxTraceSpeed { get; private set; }
	public ZombieBody HitBody { get; private set; }
	public Vector3 NetworkPosition { get; set; }
	public float PositionRefreshDiff { get; private set; }

	private void Awake()
	{
		FallAsleepMask = LayerMask.GetMask("FallAsleepObject");
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();

		Rigidbody[] bodys = GetComponentsInChildren<Rigidbody>();
		foreach(Rigidbody body in bodys)
		{
			body.isKinematic = true;
			body.detectCollisions = false;
			Collider col = body.GetComponent<Collider>();
			col.isTrigger = true;
		}

		object[] data = photonView.InstantiationData;
		if(data != null)
		{
			int idx = 0;
			Random.InitState((int)data[idx++]);
			int skinIdx = Random.Range(0, skins.childCount);

			Agent.enabled = true;
			int cnt = 0;
			foreach (Transform child in skins)
			{
				if (cnt == skinIdx)
					child.gameObject.SetActive(true);
				else
					child.gameObject.SetActive(false);
				cnt++;
			}

			SetAnimFloat("IdleShifter", Random.Range(0, 3));
			SetAnimFloat("WalkShifter", Random.Range(0, 5));
			SetAnimFloat("RunShifter", Random.Range(0, 2));
			SetAnimFloat("SprintShifter", Random.Range(0, 2));

			MaxTraceSpeed = Random.Range(1, 4);
			Target = PhotonNetwork.GetPhotonView((int)data[idx++]).transform;
		}

		stateMachine = GetComponent<StateMachine>();
		if (stateMachine == null)
			stateMachine = gameObject.AddComponent<StateMachine>();

		stateMachine.AddState(State.Idle, new ZombieIdle(this));
		stateMachine.AddState(State.Trace, new ZombieTrace(this));
		stateMachine.AddState(State.Wander, new ZombieWander(this));
		stateMachine.AddState(State.AnimWait, new ZombieAnimWait(this));
		stateMachine.AddState(State.CrawlIdle, new ZombieCrawlIdle(this));
		stateMachine.AddState(State.Wait, new ZombieWait(this));

		stateMachine.InitState(State.Idle);
	}

	//private IEnumerator Start()
	//{
	//	yield return new WaitForSeconds(1f);

	//	anim.enabled = false;
	//	Agent.enabled = false;
	//	Rigidbody[] bodys = GetComponentsInChildren<Rigidbody>();
	//	bool pushed = false;
	//	foreach (Rigidbody body in bodys)
	//	{
	//		body.isKinematic = false;
	//		body.detectCollisions = true;
	//		Collider col = body.GetComponent<Collider>();
	//		col.isTrigger = false;
	//		if(pushed == false)
	//		{
	//			body.AddForce((-transform.forward + transform.up * 0.3f) * 400f, ForceMode.Impulse);
	//			pushed = true;
	//		}
	//	}
	//}

	public void Update()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine($"현재 상태: {stateMachine.curStateStr}");
		sb.AppendLine($"SpeedX : {anim.GetFloat("SpeedX"):#.##}");
		sb.AppendLine($"SpeedY : {anim.GetFloat("SpeedY"):#.##}");
		sb.AppendLine($"posDiff : {(NetworkPosition - transform.position).magnitude:##.#####}");

		PositionRefreshDiff = Mathf.Lerp(0.01f, 0.5f, anim.GetFloat("SpeedY") * 0.3f);

		curStateText.text = sb.ToString();
		////if (Object.IsProxy)
		//{
		//	if((transform.position - Position).sqrMagnitude > 1f)
		//	{
		//		//debugCapsule.transform.position = transform.position;
		//		//debugCapsule.SetActive(true);
		//		Agent.enabled = false;
		//		transform.position = Position;
		//		Agent.enabled = true;
		//	}
		//	transform.rotation = Rotation;

		//	if(VisualHitCnt < HitCnt)
		//	{
		//		VisualHitCnt = HitCnt;
		//		anim.SetFloat("HitBodyType", GetHitBodyFloat(HitBody));
		//		anim.SetTrigger("Hit");
		//	}
		//}
		////else
		//{
		//	if (VisualHitCnt < HitCnt)
		//	{
		//		SetAnimFloat("HitBodyType", GetHitBodyFloat(HitBody));
		//		SetAnimTrigger("Hit");
		//		AnimWaitStruct = new AnimWaitStruct("StandHit", "Idle",
		//			updateAction: () => SetAnimFloat("SpeedY", 0f, 0.1f));
		//		stateMachine.ChangeState(Zombie.State.AnimWait);

		//		VisualHitCnt = HitCnt;
		//	}
		//}
	}

	public float GetHitBodyFloat(ZombieBody zombieBody)
	{
		float hitBodyType = 0f;
		switch (HitBody)
		{
			case ZombieBody.Head:
			case ZombieBody.MiddleSpine:
			case ZombieBody.Pelvis:
				hitBodyType = 0f;
				break;
			case ZombieBody.LeftArm:
			case ZombieBody.LeftElbow:
				hitBodyType = 1f;
				break;
			case ZombieBody.RightArm:
			case ZombieBody.RightElbow:
				hitBodyType = 2f;
				break;
			case ZombieBody.LeftHips:
			case ZombieBody.LeftKnee:
				hitBodyType = 3f;
				break;
			case ZombieBody.RightHips:
			case ZombieBody.RightKnee:
				hitBodyType = 4f;
				break;
			default:
				Debug.LogError($"ZombieBody 예외 처리 안됨: {HitBody}");
				break;
		}
		return hitBodyType;
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

	public void ApplyDamage(ZombieBody bodyType, Vector3 dir, float power, int damage)
	{
		HitBody = bodyType;
	}

	[PunRPC]
	public void SetTracePath(Vector3 startPos, Vector3 endPos)
	{
		NavMeshPath path = new();
		NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, path);
		agent.path = path;
	}

	public void CallChangeState(State nextState)
	{
		stateMachine.ChangeState(State.Wait);
		photonView.RPC(nameof(ChangeStateRPC), RpcTarget.AllViaServer, nextState, transform.position, transform.rotation);
	}

	[PunRPC]
	public void ChangeStateRPC(State nextState, Vector3 position, Quaternion rotation)
	{
		agent.enabled = false;
		transform.position = position;
		transform.rotation = rotation;
		agent.enabled = true;
		stateMachine.ChangeState(nextState);
	}

	[PunRPC]
	public void FallAsleepRPC(float fallAsleepValue)
	{
		SetAnimFloat("FallAsleep", fallAsleepValue);
		SetAnimBool("Crawl", true);
		AnimWaitStruct = new AnimWaitStruct("Fall", State.CrawlIdle.ToString(),
			updateAction: () => SetAnimFloat("SpeedY", 0f, 0.3f));
		stateMachine.ChangeState(State.AnimWait);
	}

	[PunRPC]
	public void TurnRPC(float turnDir)
	{
		SetAnimFloat("TurnDir", turnDir);
		SetAnimTrigger("Turn");
		AnimWaitStruct = new AnimWaitStruct("Turn", State.Trace.ToString());
		stateMachine.ChangeState(State.AnimWait);
	}
}