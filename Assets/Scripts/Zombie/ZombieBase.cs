using System.Collections;
using UnityEngine;
using Fusion;
using UnityEngine.AI;
using System;
using System.Text;
using TMPro;
using Unity.AI.Navigation;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public struct BoneTransform
{
	public Vector3 localPosition;
	public Quaternion localRotation;
}

public abstract class ZombieBase : NetworkBehaviour
{
	[SerializeField] TextMeshProUGUI curStateText;

	public struct BodyPart
	{
		public ZombieHitBox zombieHitBox;
		public Rigidbody rb;
		public Collider col;
	}

	[SerializeField] protected float mass = 40f;
	[SerializeField] protected float viewAngle = 45f;
	[SerializeField] protected float lookDist = 10f;
	[SerializeField] protected float playerLostTime = 5f;
	[SerializeField] protected GameObject headBloodVFX;
	[SerializeField] protected GameObject bodyBloodVFX;
	[SerializeField] protected int maxHp = 400;

	private TickTimer destinationTimer;
	public TickTimer PlayerFindTimer { get; set; }

	protected NavMeshAgent agent;
	protected NetworkStateMachine stateMachine;
	protected Animator anim;
	protected Collider[] overlapCols = new Collider[5];

	protected BodyPart[] bodyHitParts = new BodyPart[(int)ZombieBody.Size];

	public string WaitName { get; set; }
	public string NextState { get; set; }
	protected string prevState;
	public event Action OnDie;

	public float Mass { get { return mass; } }
	public bool IsDead { get; private set; }
	public BodyPart[] BodyHitParts { get { return bodyHitParts; } }
	public Transform Hips { get; private set; }
	public Transform Eyes { get; private set; }
	public Animator Anim { get { return anim; } }
	public NavMeshAgent Agent { get { return agent; } }
	public AnimWaitStruct? AnimWaitStruct { get; set; }

	#region LayerAndMask
	public LayerMask EnvironMask { get; private set; }
	public LayerMask PlayerMask { get; private set; }
	public LayerMask AttackTargetMask { get; private set; }
	public LayerMask HittableMask { get; private set; }
	public LayerMask FindObstacleMask { get; private set; }
	//public int PlayerLayer { get; private set; }
	//public int VehicleLayer { get; private set; }
	#endregion

	public TargetData TargetData { get; private set; }

	public int CurHp { get; protected set; }
	public int MaxHP { get { return maxHp; } }
	public bool SyncTransfrom { get; set; } = true;

	[Networked] public Vector3 Position { get; private set; }
	[Networked] public Quaternion Rotation { get; private set; }
	[Networked] public ZombieBody HitBody { get; private set; }
	[Networked] public Vector3 HitPoint { get; private set; }
	[Networked] public Vector3 HitForce { get; private set; }
	[Networked, OnChangedRender(nameof(PlayHitFX))] public int HitCnt { get; set; }

	public abstract string DecideState();

	protected virtual void Awake()
	{
		TargetData = new TargetData(transform);
		CurHp = MaxHP;
		EnvironMask = LayerMask.GetMask("Default", "EnvironMask");
		PlayerMask = LayerMask.GetMask("Player");
		HittableMask = LayerMask.GetMask("Player", "Vehicle", "Breakable");
		AttackTargetMask = LayerMask.GetMask("Player", "Vehicle");
		FindObstacleMask = LayerMask.GetMask("Default", "Environment", "Vehicle", "Breakable");
		//PlayerLayer = LayerMask.NameToLayer("Player");
		//VehicleLayer = LayerMask.NameToLayer("Vehicle");
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
		bool[] dupCheck = new bool[(int)ZombieBody.Size];
		foreach (ZombieHitBox hitBox in hitBoxes)
		{
			int bodyTypeInt = (int)hitBox.BodyType;
			if (dupCheck[bodyTypeInt] == true)
			{
				Debug.LogError($"{gameObject.name}의 {hitBox.BodyType}이 중복됩니다");
				continue;
			}
			dupCheck[bodyTypeInt] = true;
			BodyPart bodyPart = new BodyPart();
			bodyPart.zombieHitBox = hitBox;
			bodyPart.rb = hitBox.RB;
			bodyPart.col = hitBox.GetComponent<Collider>();
			bodyHitParts[bodyTypeInt] = bodyPart;
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
		TargetData.UpdateTargetData();
	}

	private void FindPlayer()
	{
		if (AttackTargetMask.IsLayerInMask(TargetData.Layer)) return;
		if (PlayerFindTimer.ExpiredOrNotRunning(Runner) == false) return;
		int result = Physics.OverlapSphereNonAlloc(transform.position, lookDist, overlapCols, PlayerMask);

		if (result == 0)
		{
			PlayerFindTimer = TickTimer.CreateFromSeconds(Runner, 1f);
			return;
		}

		Transform findPlayer = overlapCols[0].transform;
		Vector3 toPlayerVec = ((findPlayer.position + Vector3.up) - Eyes.position);
		Vector3 toPlayerDir = toPlayerVec.normalized;
		float length = toPlayerVec.magnitude;
		if (Vector3.Dot(toPlayerDir, Eyes.forward) > Mathf.Cos(viewAngle * Mathf.Deg2Rad))
		{
			if (Physics.Raycast(Eyes.position, toPlayerDir, length, FindObstacleMask) == false)
			{
				TargetData.SetTarget(overlapCols[0].transform, Runner.Tick);
				if (agent.enabled)
					agent.ResetPath();
			}
		}

		PlayerFindTimer = TickTimer.CreateFromSeconds(Runner, 0.1f);
	}

	private void TargetManage()
	{
		if (TargetData.IsTargeting == false) return;

		if (destinationTimer.ExpiredOrNotRunning(Runner))
		{
			destinationTimer = TickTimer.CreateFromSeconds(Runner, 0.5f);
			if (agent.enabled == true)
			{
				agent.SetDestination(TargetData.Position);
			}
		}

		if (AttackTargetMask.IsLayerInMask(TargetData.Layer) == false) return;

		Vector3 toTargetVec = ((TargetData.Position + Vector3.up) - Eyes.position);
		float toTargetMag = toTargetVec.magnitude;
		if (toTargetMag < lookDist * 2f)
		{
			if (Physics.Raycast(Eyes.position, toTargetVec.normalized, toTargetMag, FindObstacleMask) == false)
			{
				TargetData.LastFindTick = Runner.Tick;
			}
		}

		if (TargetData.LastFindTick + playerLostTime * Runner.TickRate < Runner.Tick)
		{
			TargetData.RemoveTarget();
		}
	}

	public override void Render()
	{
		string curState = stateMachine.curStateStr;
		StringBuilder sb = new StringBuilder();
		string printState = curState.Equals("AnimWait") ? $"{prevState} -> AnimWait({WaitName}) -> {NextState}" : curState;
		sb.AppendLine($"현재 상태: {printState}");
		sb.AppendLine($"마지막 타겟 발견시간: {((TargetData.LastFindTick - Runner.Tick) / (float)Runner.TickRate).ToString("F1")}");
		sb.AppendLine($"Target: {(TargetData.IsTargeting == false ? "None" : LayerMask.LayerToName(TargetData.Layer))}");
		sb.AppendLine($"CurHP: {CurHp}");
		sb.AppendLine($"SpeedX : {anim.GetFloat("SpeedX"):#.##}");
		sb.AppendLine($"SpeedY : {anim.GetFloat("SpeedY"):#.##}");
		sb.AppendLine($"PosDiff: {(transform.position - Position).sqrMagnitude.ToString("F4")}");
		if (curState.Equals("AnimWait") == false)
			prevState = curState;

		curStateText.text = sb.ToString();

		if (Object.IsProxy && SyncTransfrom == true)
		{
			if ((transform.position - Position).sqrMagnitude > Mathf.Lerp(0.01f, 1f, anim.GetFloat("SpeedY") * 0.2f))
			{
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
		GameManager.Resource.Instantiate(vfx, HitPoint,
			Quaternion.LookRotation(-HitForce), true);
	}

	public virtual void ApplyDamage(Transform source, ZombieHitBox zombieHitBox,
		Vector3 point, Vector3 force, int damage, bool playHitVFX = true)
	{
		HitForce = force;
		HitBody = zombieHitBox.BodyType;
		HitPoint = point;
		if (playHitVFX)
		{
			HitCnt++;
		}

		if (Object.IsProxy) return;

		TargetData.SetTarget(source, Runner.Tick);

		CurHp -= damage;
		if (CurHp <= 0)
		{
			CurHp = 0;
			if (IsDead == false)
			{
				IsDead = true;
				OnDie?.Invoke();
			}
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
			LookToward(lookDir, rotateSpeed);
			Vector3 moveDir = agent.desiredVelocity.normalized;
			Vector3 animDir = transform.InverseTransformDirection(moveDir);

			speedX = animDir.x * speed;
			speedY = animDir.z * speed;
		}
		SetAnimFloat("SpeedX", speedX, dampX);
		SetAnimFloat("SpeedY", speedY, dampY);
	}

	public void LookToward(Vector3 direction, float speed)
	{
		transform.rotation = Quaternion.RotateTowards(transform.rotation,
			Quaternion.LookRotation(direction), speed * Runner.DeltaTime);
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

	public float GetAnimNormalTime(int layer = 0)
	{
		return anim.GetCurrentAnimatorStateInfo(layer).normalizedTime;
	}

	public void Decelerate()
	{
		SetAnimFloat("SpeedX", 0f, 0.5f, Runner.DeltaTime);
		SetAnimFloat("SpeedY", 0f, 0.5f, Runner.DeltaTime);
	}

	public void Decelerate(float dampTime = 0.5f, float? deltaTime = null)
	{
		if (deltaTime.HasValue == false)
		{
			deltaTime = Runner.DeltaTime;
		}

		SetAnimFloat("SpeedX", 0f, dampTime, deltaTime);
		SetAnimFloat("SpeedY", 0f, dampTime, deltaTime);
	}

	public void SetWanderDestination(float range)
	{
		Vector3 pos = transform.position;
		Vector3 randPos = pos + Random.insideUnitSphere * range;
		randPos.y = pos.y;
		Agent.SetDestination(randPos);
	}

	public bool CheckProjectile(Vector3 firePos, Vector3 targetPos, out Vector3 velocity,
		float radius, float speed, Vector3 gravity, LayerMask obscuredMask, int segment = 4)
	{
		Vector3 diff = targetPos - firePos;
		float dist = diff.magnitude;
		float arriveTime = dist / speed;
		velocity = (targetPos - firePos) / arriveTime;
		velocity.y = (targetPos.y - firePos.y) / arriveTime
			+ (arriveTime * -gravity.y) * 0.5f;

		Vector3 curPos = firePos;
		Vector3 nextPos;

		List<Vector3> posList = new List<Vector3>();
		for (int i = 1; i <= segment; i++)
		{
			float ratio = (float)i / segment;
			float time = ratio * arriveTime;
			nextPos = firePos + (velocity + (gravity * time * 0.5f)) * time;

			Vector3 rayVel = nextPos - curPos;
			Vector3 dir = rayVel.normalized;
			float mag = rayVel.magnitude;

			posList.Add(curPos);
			posList.Add(nextPos);

			if (i != segment)
			{
				if (Physics.SphereCast(curPos, radius, dir, out RaycastHit hit, mag, obscuredMask) == true)
				{
					return false;
				}
			}
			else
			{
				if (Physics.Raycast(curPos, dir, mag - 1f, obscuredMask) == true)
				{
					return false;
				}
			}
			curPos = nextPos;
		}
		return true;
	}
}