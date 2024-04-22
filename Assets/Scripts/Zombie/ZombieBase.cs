using System.Collections;
using UnityEngine;
using Fusion;
using UnityEngine.AI;
using System;
using static UnityEngine.UI.GridLayoutGroup;

public struct BoneTransform
{
	public Vector3 localPosition;
	public Quaternion localRotation;
}

public abstract class ZombieBase : NetworkBehaviour
{
	public struct BodyPart
	{
		public ZombieHitBox zombieHitBox;
		public Rigidbody rb;
		public Collider col;
	}

	public enum TargetType { None, Meat, Player }

	[SerializeField] protected float viewAngle = 45f;
	[SerializeField] protected float lookDist = 10f;
	[SerializeField] protected float playerLostTime = 5f;
	[SerializeField] protected GameObject headBloodVFX;
	[SerializeField] protected GameObject bodyBloodVFX;

	private TickTimer destinationTimer;
	private TickTimer playerFindTimer;

	protected NavMeshAgent agent;
	protected NetworkStateMachine stateMachine;
	protected Animator anim;
	protected Collider[] overlapCols = new Collider[5];

	protected BodyPart[] bodyHitParts = new BodyPart[(int)ZombieBody.Size];

	public Action OnHit;

	public string WaitName { get; set; }
	public string NextState { get; set; }
	protected string prevState;

	public BodyPart[] BodyHitParts { get { return bodyHitParts; } }
	public Transform Hips { get; private set; }
	public Transform Eyes { get; private set; }
	public Animator Anim { get { return anim; } }
	public NavMeshAgent Agent { get { return agent; } }
	public LayerMask PlayerMask { get; private set; }
	public int PlayerLayer { get; set; }
	public AnimWaitStruct? AnimWaitStruct { get; set; }
	public TargetType CurTargetType { get; set; }
	public Transform Target { get; set; }
	public Tick LastPlayerFindTick { get; protected set; }
	public int CurHp { get; protected set; }
	public int MaxHP { get; protected set; } = 300;

	[Networked] public Vector3 Position { get; private set; }
	[Networked] public Quaternion Rotation { get; private set; }
	[Networked] public ZombieBody HitBody { get; private set; }
	[Networked] public Vector3 HitVelocity { get; private set; }
	[Networked, OnChangedRender(nameof(PlayHitFX))] public int HitCnt { get; set; }

	public abstract string DecideState();

	protected virtual void Awake()
	{
		CurHp = MaxHP;
		PlayerMask = LayerMask.GetMask("Player");
		PlayerLayer = LayerMask.NameToLayer("Player");
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();

		Hips = anim.GetBoneTransform(HumanBodyBones.Hips);
		Eyes = anim.GetBoneTransform(HumanBodyBones.LeftEye);

		stateMachine = GetComponent<NetworkStateMachine>();

		SetBodyParts();
	}

	private void SetBodyParts()
	{
		ZombieHitBox[] hitBoxes = GetComponentsInChildren<ZombieHitBox>();
		foreach (ZombieHitBox hitBox in hitBoxes)
		{
			BodyPart bodyPart = new BodyPart();
			bodyPart.zombieHitBox = hitBox;
			bodyPart.rb = hitBox.RB;
			bodyPart.col = hitBox.GetComponent<Collider>();
			bodyHitParts[(int)hitBox.BodyType] = bodyPart;
		}
	}

	public override void Spawned()
	{
		Agent.enabled = true;
	}

	public override void FixedUpdateNetwork()
	{
		Position = transform.position;
		Rotation = transform.rotation;

		FindPlayer();
		TargetManage();
	}

	private void FindPlayer()
	{
		if (CurTargetType == TargetType.Player) return;
		if (playerFindTimer.ExpiredOrNotRunning(Runner) == false) return;

		int result = Physics.OverlapSphereNonAlloc(transform.position, lookDist, overlapCols, PlayerMask);

		if (result == 0)
		{
			playerFindTimer = TickTimer.CreateFromSeconds(Runner, 1f);
			return;
		}

		Transform findPlayer = overlapCols[0].transform;
		Vector3 toPlayerVec = ((findPlayer.position + Vector3.up) - Eyes.position);
		Vector3 toPlayerDir = toPlayerVec.normalized;
		float length = toPlayerVec.magnitude;
		if (Vector3.Dot(toPlayerDir, Eyes.forward) > Mathf.Cos(viewAngle * Mathf.Deg2Rad))
		{
			if (Physics.Raycast(Eyes.position, toPlayerDir, length, LayerMask.GetMask("Default")) == false)
			{
				LastPlayerFindTick = Runner.Tick;
				Target = overlapCols[0].transform;
				CurTargetType = TargetType.Player;
				agent.ResetPath();
			}
		}

		playerFindTimer = TickTimer.CreateFromSeconds(Runner, 0.1f);
	}

	private void TargetManage()
	{
		if (Target == null) return;

		if (destinationTimer.ExpiredOrNotRunning(Runner))
		{
			destinationTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
			if (Target != null && agent.enabled == true)
				agent.SetDestination(Target.position);
		}

		if (CurTargetType != TargetType.Player) return;

		Vector3 toPlayerVec = ((Target.position + Vector3.up) - Eyes.position);
		if (Physics.Raycast(Eyes.position, toPlayerVec.normalized, toPlayerVec.magnitude, LayerMask.GetMask("Default")) == false)
		{
			LastPlayerFindTick = Runner.Tick;
		}

		if (LastPlayerFindTick + playerLostTime * Runner.TickRate < Runner.Tick)
		{
			Target = null;
			CurTargetType = TargetType.None;
		}
	}

	public override void Render()
	{
		if (Object.IsProxy)
		{
			if ((transform.position - Position).sqrMagnitude > Mathf.Lerp(0.01f, 1f, anim.GetFloat("SpeedY") * 0.2f))
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

	protected void PlayHitFX()
	{
		GameObject vfx = HitBody == ZombieBody.Head ? headBloodVFX : bodyBloodVFX;
		GameManager.Resource.Instantiate(vfx, bodyHitParts[(int)HitBody].col.bounds.center, 
			Quaternion.LookRotation(-HitVelocity), true);
	}

	public virtual void ApplyDamage(Transform source, ZombieHitBox zombieHitBox, 
		Vector3 velocity, int damage, bool playHitVFX = true)
	{
		OnHit?.Invoke();

		HitVelocity = velocity;
		HitBody = zombieHitBox.BodyType;
		if(playHitVFX)
		{
			HitCnt++;
		}

		if (Object.IsProxy) return;

		Target = source;
		CurTargetType = TargetType.Player;
		LastPlayerFindTick = Runner.Tick;

		CurHp -= damage;
		if (CurHp <= 0)
		{
			CurHp = 0;
		}
	}

	public void Trace(float speed, float rotateSpeed, float dampX, float dampY)
	{
		float speedX = 0f;
		float speedY = 0f;
		if (agent.hasPath)
		{
			Vector3 lookDir = (agent.steeringTarget - transform.position);
			lookDir.y = 0f;
			lookDir.Normalize();
			transform.rotation = Quaternion.RotateTowards(transform.rotation,
				Quaternion.LookRotation(lookDir), rotateSpeed * Runner.DeltaTime);
			Vector3 moveDir = agent.desiredVelocity.normalized;
			Vector3 animDir = transform.InverseTransformDirection(moveDir);

			speedX = animDir.x * speed;
			speedY = animDir.z * speed;
		}
		SetAnimFloat("SpeedX", speedX, dampX);
		SetAnimFloat("SpeedY", speedY, dampY);
	}

	public virtual void Heal(int amount, bool healLeg = true)
	{
		CurHp += amount;
		CurHp = Mathf.Clamp(CurHp, 0, MaxHP);
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
		if (deltaTime.HasValue == false)
		{
			deltaTime = Runner.DeltaTime;
		}
		anim.SetFloat(name, value, dampTime, deltaTime.Value);
	}

	public void SetAnimTrigger(string name)
	{
		anim.SetTrigger(name);
	}

	public void SetAnimInt(string name, int value)
	{
		anim.SetInteger(name, value);
	}

	public bool IsAnimName(string name, int layer = 0)
	{
		return anim.GetCurrentAnimatorStateInfo(layer).IsName(name);
	}
	
	public void Decelerate()
	{
		SetAnimFloat("SpeedX", 0f, 0.5f, Runner.DeltaTime);
		SetAnimFloat("SpeedY", 0f, 0.5f, Runner.DeltaTime);
	}

	public void Decelerate(float dampTime = 0.5f, float? deltaTime = null)
	{
		if(deltaTime.HasValue == false)
		{
			deltaTime = Runner.DeltaTime;
		}

		SetAnimFloat("SpeedX", 0f, dampTime, deltaTime);
		SetAnimFloat("SpeedY", 0f, dampTime, deltaTime);
	}
}