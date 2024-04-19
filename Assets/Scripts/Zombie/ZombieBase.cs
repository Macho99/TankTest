using System.Collections;
using UnityEngine;
using Fusion;
using UnityEngine.AI;

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

	[SerializeField] protected float viewAngle = 60f;
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

	public BodyPart[] BodyHitParts { get { return bodyHitParts; } }
	public Transform Hips { get; private set; }
	public Transform Head { get; private set; }
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

	protected virtual void Awake()
	{
		CurHp = MaxHP;
		PlayerMask = LayerMask.GetMask("Player");
		PlayerLayer = LayerMask.NameToLayer("Player");
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();

		Hips = anim.GetBoneTransform(HumanBodyBones.Hips);
		Head = anim.GetBoneTransform(HumanBodyBones.Head);

		stateMachine = GetComponent<NetworkStateMachine>();

		SetBodyParts();
		SetRbKinematic(true);
	}

	public override void Spawned()
	{
		base.Spawned();
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

		int result = Physics.OverlapSphereNonAlloc(transform.position, 10f, overlapCols, PlayerMask);

		if (result == 0)
		{
			playerFindTimer = TickTimer.CreateFromSeconds(Runner, 1f);
			return;
		}

		Transform findPlayer = overlapCols[0].transform;
		Vector3 toPlayerVec = ((findPlayer.position + Vector3.up) - Head.position);
		if (Vector3.Dot(toPlayerVec, Head.forward) > Mathf.Cos(viewAngle * Mathf.Deg2Rad))
		{
			if (Physics.Raycast(Head.position, toPlayerVec.normalized, toPlayerVec.magnitude, LayerMask.GetMask("Default")) == false)
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

		Vector3 toPlayerVec = ((Target.position + Vector3.up) - Head.position);
		if (Physics.Raycast(Head.position, toPlayerVec.normalized, toPlayerVec.magnitude, LayerMask.GetMask("Default")) == false)
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

	private void SetBodyParts()
	{
		ZombieHitBox[] hitBoxes = GetComponentsInChildren<ZombieHitBox>();
		foreach (ZombieHitBox hitBox in hitBoxes)
		{
			BodyPart bodyPart = new BodyPart();
			bodyPart.zombieHitBox = hitBox;
			bodyPart.rb = hitBox.GetComponent<Rigidbody>();
			bodyPart.col = hitBox.GetComponent<Collider>();
			bodyHitParts[(int)hitBox.BodyType] = bodyPart;
		}
	}

	public void SetRbKinematic(bool value)
	{
		foreach (BodyPart bodyPart in bodyHitParts)
		{
			if (bodyPart.rb == null)
				return;
			bodyPart.rb.isKinematic = value;
			bodyPart.rb.detectCollisions = !value;
			bodyPart.col.isTrigger = value;
		}
	}

	protected void PlayHitFX()
	{
		GameObject vfx = HitBody == ZombieBody.Head ? headBloodVFX : bodyBloodVFX;
		Instantiate(vfx, bodyHitParts[(int)HitBody].zombieHitBox.transform.position,
			Quaternion.LookRotation(-HitVelocity));
	}

	public virtual void ApplyDamage(Transform source, ZombieHitBox zombieHitBox, Vector3 velocity, int damage)
	{
		HitVelocity = velocity;
		HitBody = zombieHitBox.BodyType;
		HitCnt++;

		if (Object.IsProxy) return;

		CurHp -= damage;
		if (CurHp <= 0)
		{
			CurHp = 0;
		}
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

	public bool IsAnimName(string name, int layer = 0)
	{
		return anim.GetCurrentAnimatorStateInfo(layer).IsName(name);
	}
}