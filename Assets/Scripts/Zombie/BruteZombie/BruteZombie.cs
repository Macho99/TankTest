using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class BruteZombie : ZombieBase
{
	[SerializeField] Transform lookTarget;
	[SerializeField] Rig lookRig;
	[SerializeField] float lookSpeed = 1.5f;
	[SerializeField] Transform shieldHolder;
	[SerializeField] Transform stoneHolder;
	[SerializeField] float normalAttackDist = 4f;
	[SerializeField] float twoHandGroundCooltime = 10f;
	[SerializeField] float dashCooltime = 20f;
	[SerializeField] float dashDist = 15f;
	[SerializeField] float jumpCooltime = 40f;
	[SerializeField] float minHeight = 0f;
	[SerializeField] float maxHeight = 5f;
	[SerializeField] float minJumpDist = 10f;
	[SerializeField] float maxJumpDist = 20f;
	[SerializeField] float stoneCooltime = 20f;
	[SerializeField] float minStoneDist = 15f;
	[SerializeField] float maxStoneDist = 30f;
	[SerializeField] float stoneSpeed = 40f;
	[SerializeField] NetworkPrefabRef stonePrefab;

	[Header("Gizmos")]
	[SerializeField] bool drawGizmos = true;
	[SerializeField] bool drawDashGizmos = true;
	[SerializeField] bool drawJumpGizmos = true;
	[SerializeField] bool drawStoneGizmos = true;

	public float TwoHandGroundCooltime { get { return twoHandGroundCooltime; } }
	public float DashCooltime { get { return dashCooltime; } }
	public float DashDist { get { return dashDist; } }
	public float JumpCooltime { get { return jumpCooltime; } }
	public float MinJumpDist { get { return minJumpDist; } }
	public float MaxJumpDist { get { return maxJumpDist; } }
	public float StoneCooltime { get { return stoneCooltime; } }
	public float MinStoneDist { get { return minStoneDist; } }
	public float MaxStoneDist { get { return maxStoneDist; } }
	public float StoneSpeed { get { return stoneSpeed; } }
	public NetworkPrefabRef StonePrefab { get { return stonePrefab; } }
	public Transform StoneHolder { get { return stoneHolder; } }
	public bool CCImmune { get; set; }

	public enum AttackType { Back = -1, LeftFoot, RightFoot, Smash, DoubleSmash, TwoHandSmash, GroundAttack, 
		Jump, Dash, TwoHandGround, ThrowStone };
	public enum State { Idle, Trace, DefenceTrace, DefenceKnockback, Stun, Search, Roar, Jump, AnimWait, Wait, Die}

	public event Action OnHit;
	public event Action OnStun;

	public TickTimer TwoHandGroundTimer { get; set; }
	public TickTimer DashTimer { get; set; }
	public TickTimer JumpTimer { get; set; }
	public TickTimer StoneTimer { get; set; }
	public TickTimer FootAttackTimer { get; set; }
	public TickTimer DefenceCooltimer { get; set; }

	public float NormalAttackDist { get { return normalAttackDist; } }
	public BruteShield Shield { get; private set; }
	public int ShieldCnt { get; set; } = 2;
	TickTimer berserkRiseTimer;

	[Networked, OnChangedRender(nameof(StoneRender))] public NetworkBool StoneActive { get; set; }
	[Networked, OnChangedRender(nameof(BerserkRender))] public NetworkBool IsBerserk { get; set; }
	[Networked] public float LookWeight { get; set; }
	[Networked] public Vector3 LookPos { get; set; }
	[Networked, OnChangedRender(nameof(JumpAttack))] public int JumpCnt { get; set; }
	[Networked] public Vector3 JumpEndPos { get; set; }

	protected override void Awake()
	{
		base.Awake();
		Shield = shieldHolder.GetComponentInChildren<BruteShield>();

		stateMachine.AddState(State.Idle, new BruteIdle(this));
		stateMachine.AddState(State.Trace, new BruteTrace(this));
		stateMachine.AddState(State.Search, new BruteSearch(this));
		stateMachine.AddState(State.DefenceTrace, new BruteDefenceTrace(this));
		stateMachine.AddState(State.DefenceKnockback, new BruteDefenceKnockback(this));
		stateMachine.AddState(State.Roar, new BruteRoar(this));
		stateMachine.AddState(State.Jump, new BruteJump(this));
		stateMachine.AddState(State.AnimWait, new ZombieAnimWait(this));
		stateMachine.AddState(State.Wait, new ZombieWait());
		stateMachine.AddState(State.Die, new ZombieBaseDie(this));

		stateMachine.InitState(State.Idle);
		OnDie += () =>
		{
			LookWeight = 0f;
			SetAnimBool("Die", true);
			SetAnimTrigger("DefenceExit");
			stateMachine.ChangeState(State.Die);
		};
	}

	public override void Spawned()
	{
		base.Spawned();
		if (IsBerserk)
		{
			Destroy(Shield.gameObject);
			Shield = null;
		}
		minimapTarget.Init(MinimapTarget.TargetType.BossMonster);
		GameManager.Instance.BruteZombieCnt++;
    }
	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		base.Despawned(runner, hasState);
		GameManager.Instance.BruteZombieCnt--;
	}

	public override void FixedUpdateNetwork()
	{
		base.FixedUpdateNetwork();
	}

	public override void Render()
	{
		base.Render();
		
		lookRig.weight = Mathf.Lerp(lookRig.weight, LookWeight, Runner.DeltaTime * lookSpeed);
		lookTarget.position = Vector3.Lerp(lookTarget.position, LookPos, Runner.DeltaTime * lookSpeed);
	}

	private void BerserkRender()
	{
		Shield.Break();
		ShieldCnt = 0;
		Shield = null;
	}

	public override string DecideState()
	{
		throw new NotImplementedException();
	}

	public void LookTarget()
	{
		if (TargetData.IsTargeting == true)
		{
			LookPos = TargetData.Position;
		}
		else
		{
			LookPos = transform.TransformPoint(Vector3.forward * 5f);
		}
	}

	public override void ApplyDamage(Transform source, ZombieHitBox zombieHitBox, 
		Vector3 point, Vector3 velocity, int damage, bool playHitVFX = true)
	{
		base.ApplyDamage(source, zombieHitBox, point, velocity, damage, playHitVFX);

		if (Object.IsProxy) return;

		if (IsDead)
			return;

		if (IsBerserk == false && CurHp < maxHp / 2)
		{
			ChangeToBerserk();
			return;
		}

		Vector3 angleVelocity = velocity;
		angleVelocity.y = 0f;
		float angle = Vector3.SignedAngle(transform.forward, angleVelocity, transform.up);
		float sign = (angle >= 0f) ? 1f : -1f;
		float absAngle = Mathf.Abs(angle);

		//총알에 피격이거나 CC면역상태일때
		if (damage < 500 || CCImmune)
		{
			float shifter;
			if(absAngle < 45f)
			{
				shifter = -1f;
			}
			else if (absAngle < 135f)
			{
				if(sign < 0f)
				{
					shifter = 1f;
				}
				else
				{
					shifter = 2f;
				}
			}
			else
			{
				shifter = 0f;
			}
			SetAnimFloat("LightHitShifter", shifter);
			SetAnimTrigger("LightHit");
		}
		//전차에 피격
		else
		{
			PlaySound(ZombieSoundType.Hit);
			//뒤쪽에서 맞았으면 잠깐 스턴
			if(absAngle < 90f)
			{
				Stun(2f);
				return;
			}

			bool left;
			switch (zombieHitBox.BodyType)
			{
				case ZombieBody.LeftArm:
				case ZombieBody.LeftElbow:
				case ZombieBody.LeftHips:
				case ZombieBody.LeftKnee:
					left = true;
					break;
				case ZombieBody.RightArm:
				case ZombieBody.RightElbow:
				case ZombieBody.RightHips:
				case ZombieBody.RightKnee:
					left = false;
					break;
				default:
					if(sign < 0f)
						left = true;
					else
						left = false;
					break;
			}

			if(left == true)
			{
				SetAnimFloat("ActionShifter", -1f);
			}
			else
			{
				SetAnimFloat("ActionShifter", 1f);
			}

			SetAnimTrigger("Hit");
			if (IsBerserk == false && Shield.Enabled == true)
			{
				stateMachine.ChangeState(State.DefenceKnockback);
			}
			else
			{
				State nextState;
				if (ShieldCnt > 0)
				{
					Shield.ResetHp();
					nextState = State.DefenceTrace;
				}
				else
				{
					nextState = State.Trace;
				}
				AnimWaitStruct = new AnimWaitStruct("Hit", nextState.ToString(),
					updateAction: Decelerate);
				stateMachine.ChangeState(State.AnimWait);
			}
		}

		OnHit?.Invoke();
	}

	bool isStun;
	bool exitTriggered;
	TickTimer durationTimer;
	public void Stun(float duration)
	{
		OnStun?.Invoke();
		if (isStun == true && exitTriggered == false)
		{
			float? nullableRemainTime = durationTimer.RemainingTime(Runner);
			float remainTime = nullableRemainTime.HasValue ? nullableRemainTime.Value : 0f;
			durationTimer = TickTimer.CreateFromSeconds(Runner, remainTime + duration);
			return;
		}

		isStun = true;
		exitTriggered = false;
		durationTimer = TickTimer.CreateFromSeconds(Runner, duration);
		SetAnimTrigger("Stun");
		AnimWaitStruct = new AnimWaitStruct("StunEnd", "Idle", startAnimName: "StunStart",
			startAction: () => LookWeight = 0f,
			updateAction: () =>
			{
				Decelerate(0.5f);
				if (exitTriggered == false && durationTimer.ExpiredOrNotRunning(Runner))
				{
					exitTriggered = true;
					SetAnimTrigger("Exit");
				}
			},
			exitAction: () => { isStun = false; LookWeight = 1f; });
		stateMachine.ChangeState(State.AnimWait);
	}

	public void ChangeToBerserk()
	{
		SetAnimInt("Defence", 0);
		SetAnimFloat("AnimSpeed", 1.5f);
		SetAnimTrigger("DefenceExit");
		SetAnimTrigger("Down");
		LookWeight = 0f;
		berserkRiseTimer = TickTimer.CreateFromSeconds(Runner, 2f);
		AnimWaitStruct = new AnimWaitStruct("Rise", State.Roar.ToString(),
			startAction: () => IsBerserk = true,
			updateAction: () => 
			{
				Decelerate();
				if (berserkRiseTimer.Expired(Runner))
				{
					berserkRiseTimer = TickTimer.None;
					SetAnimTrigger("Exit");
				}
			}, 
			exitAction: () => LookWeight = 1f);
		stateMachine.ChangeState(State.AnimWait);
	}

	public float GetJumpHeight(float dist)
	{
		return Mathf.Lerp(minHeight, maxHeight, (dist - minJumpDist) / (maxJumpDist - minJumpDist));
	}

	private void JumpAttack()
	{
		stateMachine.ChangeState(State.Jump);
	}

	private void StoneRender()
	{
		StoneHolder.gameObject.SetActive(StoneActive);
	}

	public Vector3[] LastJumpLines { get; set; }
	private void OnDrawGizmos()
	{
		if(drawGizmos == false) return;

		Gizmos.DrawWireSphere(transform.position, lookDist * eyeSightRatio);

		//근거리 공격
		Gizmos.DrawWireSphere(transform.position, normalAttackDist);
		Vector3 offset = Vector3.up;

		if(drawDashGizmos == true)
		{
			//돌진 공격
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(transform.position + offset,
				transform.position + Quaternion.Euler(0f, -5f, 0f) * transform.forward * dashDist + offset);
			Gizmos.DrawLine(transform.position + offset,
				transform.position + Quaternion.Euler(0f, 5f, 0f) * transform.forward * dashDist + offset);
		}

		Vector3 targetDir;
		if (TargetData == null || TargetData.IsTargeting == false)
		{
			targetDir = transform.forward;
		}
		else
		{
			targetDir = TargetData.Position - transform.position;
			if (drawJumpGizmos == true){
				Gizmos.color = Color.blue;

				Vector3 jumpLineOffset = Vector3.up * 2f;
				Vector3 startPos = transform.position + jumpLineOffset;
				Vector3 endPos = TargetData.Position + jumpLineOffset;
				float jumpHeight = GetJumpHeight((startPos - endPos).magnitude);

				Vector3 curPos = startPos;
				Vector3 nextPos;
				int segment = 4;

				float ratio;
				for (int i = 1; i <= segment; i++)
				{
					ratio = (float)i / segment;
					nextPos = Vector3.Lerp(startPos, endPos, ratio);
					nextPos.y += jumpHeight * Mathf.Sin(ratio * 180f * Mathf.Deg2Rad);

					Gizmos.DrawLine(curPos, nextPos);

					curPos = nextPos;
				}
			}

			if(drawStoneGizmos == true){
				Gizmos.color = Color.gray;
				Vector3 targetPos = TargetData.Position;
				Vector3 ownerPosition = transform.position + Vector3.up * 3f;
				float arriveTime = (targetPos - ownerPosition).magnitude / StoneSpeed;
				Vector3 stoneVelocity = (targetPos - ownerPosition) / arriveTime;
				stoneVelocity.y = (targetPos.y - ownerPosition.y) / arriveTime
					+ (arriveTime * -Physics.gravity.y) * 0.5f;

				int segment = 4;
				Vector3 curPos = ownerPosition;
				Vector3 nextPos;

				List<Vector3> posList = new List<Vector3>();
				for (int i = 1; i <= segment; i++)
				{
					float ratio = (float)i / segment;
					float time = ratio * arriveTime;
					nextPos = ownerPosition + (stoneVelocity + (Physics.gravity * time * 0.5f)) * time;

					posList.Add(curPos);
					posList.Add(nextPos);
					Gizmos.DrawWireSphere(curPos, 1.5f);
					Gizmos.DrawLine(curPos, nextPos);

					curPos = nextPos;
				}
			}
		}
		targetDir.y = 0f;
		targetDir.Normalize();

		if (drawJumpGizmos)
		{
			//점프 공격 거리
			offset += Vector3.up * 0.5f;
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(transform.position + offset + targetDir * minJumpDist,
				transform.position + offset + targetDir * maxJumpDist);

			Gizmos.color = Color.cyan;
			if (LastJumpLines != null)
			{
				Gizmos.DrawLineList(LastJumpLines);
			}
		}


		if (drawStoneGizmos)
		{
			//돌 던지기 공격 거리
			Gizmos.color = Color.green;
			offset += Vector3.up * 0.5f;
			Gizmos.DrawLine(transform.position + offset + targetDir * minStoneDist,
				transform.position + offset + targetDir * maxStoneDist);

			Gizmos.color = Color.magenta;
			//if (LastStoneLines != null)
			//{
			//	Gizmos.DrawLineList(LastStoneLines);
			//	for (int i = 0; i < LastStoneLines.Length; i += 2)
			//	{
			//		Gizmos.DrawWireSphere(LastStoneLines[i], 1.5f);
			//	}
			//}
		}
	}
}