using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using static UnityEngine.UI.GridLayoutGroup;

public class BruteZombie : ZombieBase
{
	[SerializeField] Transform lookTarget;
	[SerializeField] Rig lookRig;
	[SerializeField] float lookSpeed = 1.5f;
	[SerializeField] Transform shieldHolder;
	[SerializeField] float normalAttackDist = 3f;
	[SerializeField] float dashDist = 15f;
	[SerializeField] float minHeight = 1f;
	[SerializeField] float maxHeight = 5f;
	[SerializeField] float minJumpDist = 10f;
	[SerializeField] float maxJumpDist = 20f;
	[SerializeField] float minStoneDist = 15f;
	[SerializeField] float maxStoneDist = 30f;
	[SerializeField] float stoneSpeed = 5f;
	[SerializeField] Rigidbody stonePrefab;
	[SerializeField] bool drawGizmos = true;

	public float DashDist { get { return dashDist; } }
	public float MinJumpDist { get { return minJumpDist; } }
	public float MaxJumpDist { get { return maxJumpDist; } }
	public float MinStoneDist { get { return minStoneDist; } }
	public float MaxStoneDist { get { return maxStoneDist; } }
	public float StoneSpeed { get { return stoneSpeed; } }
	public Rigidbody StonePrefab { get { return stonePrefab; } }

	public enum AttackType { Back = -1, LeftFoot, RightFoot, Smash, DoubleSmash, TwoHandSmash, GroundAttack, 
		Jump, Dash, TwoHandGround, ThrowStone };
	public enum State { Idle, Trace, DefenceTrace, DefenceKnockback, Stun, Search, Roar, Jump, AnimWait, Wait, }

	public event Action OnHit;
	public event Action OnStun;

	public TickTimer SpecialAttackTimer { get; set; }

	public float NormalAttackDist { get { return normalAttackDist; } }
	public BruteShield Shield { get; private set; }
	public int ShieldCnt { get; set; } = 2;
	TickTimer berserkRiseTimer;

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

		stateMachine.InitState(State.Idle);
	}

	public override void Spawned()
	{
		base.Spawned();
		if (IsBerserk)
		{
			Destroy(Shield.gameObject);
			Shield = null;
		}
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
		if (Target != null)
		{
			LookPos = Target.position;
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

		if(IsBerserk == false && CurHp < maxHp / 2)
		{
			ChangeToBerserk();
			return;
		}

		Vector3 angleVelocity = velocity;
		angleVelocity.y = 0f;
		float angle = Vector3.SignedAngle(transform.forward, angleVelocity, transform.up);
		float sign = (angle >= 0f) ? 1f : -1f;
		float absAngle = Mathf.Abs(angle);

		//총알에 피격
		if (damage < 100)
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

	public Vector3[] LastAirLines { get; set; }
	private void OnDrawGizmos()
	{
		if(drawGizmos == false) return;

		Gizmos.DrawWireSphere(transform.position, lookDist);

		//근거리 공격
		Gizmos.DrawWireSphere(transform.position, normalAttackDist);
		Vector3 offset = Vector3.up;

		//돌진 공격
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(transform.position + offset, 
			transform.position + Quaternion.Euler(0f, -5f, 0f) * transform.forward * dashDist + offset);
		Gizmos.DrawLine(transform.position + offset, 
			transform.position + Quaternion.Euler(0f, 5f, 0f) * transform.forward * dashDist + offset);

		Vector3 targetDir;
		if (Target == null)
		{
			targetDir = transform.forward;
		}
		else
		{
			Gizmos.color = Color.blue;
			targetDir = Target.position - transform.position;

			Vector3 jumpLineOffset = Vector3.up * 2f;
			Vector3 startPos = transform.position + jumpLineOffset;
			Vector3 endPos = Target.position + jumpLineOffset;
			float jumpHeight = GetJumpHeight((startPos - endPos).magnitude);

			Vector3 curPos = startPos;
			Vector3 nextPos;
			int segment = 4;

			float ratio;
			for (int i = 1; i <= segment; i++)
			{
				ratio = (float) i / segment;
				nextPos = Vector3.Lerp(startPos, endPos, ratio);
				nextPos.y += jumpHeight * Mathf.Sin(ratio * 180f * Mathf.Deg2Rad);

				Gizmos.DrawLine(curPos, nextPos);

				curPos = nextPos;
			}
		}
		targetDir.y = 0f;
		targetDir.Normalize();

		//점프 공격 거리
		offset += Vector3.up * 0.5f;
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position + offset + targetDir * minJumpDist,
			transform.position + offset + targetDir * maxJumpDist);


		//돌 던지기 공격 거리
		offset += Vector3.up * 0.5f;
		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position + offset + targetDir * minStoneDist,
			transform.position + offset + targetDir * maxStoneDist);

		Gizmos.color = Color.cyan;
		//마지막 공중 공격 경로
		if (LastAirLines != null)
		{
			Gizmos.DrawLineList(LastAirLines);
		}
	}
}