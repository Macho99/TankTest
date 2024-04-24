using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class BruteTrace : BruteZombieState
{
	const float speed = 1f;
	const float rotateSpeed = 60f;

	Vector3 stoneVelocity;

	public BruteTrace(BruteZombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		if(owner.IsBerserk == false)
			owner.OnHit += ChangeToDefence;
		if (CheckTransition() == true)
		{
			return;
		}
	}

	public override void Exit()
	{
		if (owner.IsBerserk == false)
			owner.OnHit -= ChangeToDefence;
	}

	private void ChangeToDefence()
	{
		owner.Shield.ResetHp();
		ChangeState(BruteZombie.State.DefenceTrace);
	}

	public override void FixedUpdateNetwork()
	{
		owner.LookTarget();
		owner.Trace(speed, rotateSpeed, 0.5f, 1f);
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		if (CheckTransition() == true)
		{
			return;
		}
	}

	private bool CheckTransition()
	{
		if (owner.Target == null)
		{
			if (owner.Agent.hasPath && owner.Agent.remainingDistance < owner.NormalAttackDist)
			{
				owner.Agent.ResetPath();
				ChangeState(BruteZombie.State.Idle);
				return true;
			}
			return false;
		}

		Vector3 attackPos = owner.transform.position;
		float dist = (attackPos - owner.Target.position).magnitude;

		//플레이어가 좁은 건물로 들어갈 경우
		//if (owner.Agent.hasPath && owner.Agent.remainingDistance < 1f)
		if (owner.Agent.pathStatus == NavMeshPathStatus.PathPartial)
		{
			if (dist > 5f)
			{
				ChangeState(BruteZombie.State.Roar);
				owner.CurTargetType = ZombieBase.TargetType.None;
				owner.Target = null;
				owner.PlayerFindTimer = TickTimer.CreateFromSeconds(owner.Runner, 10f);
				owner.Agent.ResetPath();
				return true;
			}
		}

		Vector3 targetDir = (owner.Target.position - owner.transform.position);
		targetDir.y = 0f;
		targetDir.Normalize();

		float angle = Vector3.SignedAngle(owner.transform.forward, targetDir, owner.transform.up);
		float sign = Mathf.Sign(angle);
		angle = Mathf.Abs(angle);

		float attackDist = owner.NormalAttackDist;
		if (owner.IsBerserk)
		{
			//특수 공격 쿨타임 되면
			if (owner.SpecialAttackTimer.ExpiredOrNotRunning(owner.Runner))
			{
				//특수 근접공격
				if(dist < attackDist && angle < 60f)
				{
					Attack(BruteZombie.AttackType.TwoHandGround, 5f);
					return true;
				}
				//돌진 공격
				if(dist < owner.DashDist && angle < 5f)
				{
					Attack(BruteZombie.AttackType.Dash, 5f);
					return true;
				}
				//점프 공격
				if(owner.MinJumpDist < dist && dist < owner.MaxJumpDist)
				{
					if(CheckJump(dist) == true)
					{
						Attack(BruteZombie.AttackType.Jump, 5f);
						return true;
					}
				}
				//돌 던지기 공격
				if (owner.MinStoneDist < dist && dist < owner.MaxStoneDist)
				{
					if(CheckStone(dist) == true)
					{
						Attack(BruteZombie.AttackType.ThrowStone, 5f);
						return true;
					}
				}
			}
		}
		if (dist < owner.NormalAttackDist)
		{
			print(angle);
			BruteZombie.AttackType attackType;
			if (angle > 90f)
			{
				attackType = BruteZombie.AttackType.Back;
			}
            else if(angle > 30f)
            {
				attackType = sign < 0f ? BruteZombie.AttackType.LeftFoot : BruteZombie.AttackType.RightFoot;
            }
			else
			{
				attackType = (BruteZombie.AttackType) Random.Range(2, 6);
			}
            Attack(attackType);
			return true;
		}
		return false;
	}

	private void Attack(BruteZombie.AttackType type, float specialAttackCooltime = -1f)
	{
		if (specialAttackCooltime > 0f)
		{
			owner.SpecialAttackTimer = TickTimer.CreateFromSeconds(owner.Runner, specialAttackCooltime);
		}

		if (type == BruteZombie.AttackType.Jump)
		{
			owner.JumpEndPos = owner.Agent.pathEndPosition;
			ChangeState(BruteZombie.State.Wait);
			owner.JumpCnt++;
			return;
		}

		owner.SetAnimFloat("ActionShifter", (int)type);
		owner.SetAnimTrigger("Attack");
		AnimWaitStruct animWait = new AnimWaitStruct("Attack", BruteZombie.State.Trace.ToString(), 
			startAction: () => owner.LookWeight = 0f,
			updateAction: owner.Decelerate, 
			exitAction: () => owner.LookWeight = 1f);


		if (type == BruteZombie.AttackType.ThrowStone)
		{
			animWait.startAction += () =>
			{
				Rigidbody stoneRb = GameObject.Instantiate(owner.StonePrefab, owner.transform.position + Vector3.up * 10f, owner.transform.rotation);
				stoneRb.velocity = stoneVelocity;
			};
		}

		owner.AnimWaitStruct = animWait;
		ChangeState(BruteZombie.State.AnimWait);
	}

	private bool CheckJump(float dist)
	{
		Vector3 offset = Vector3.up * 2f;
		Vector3 startPos = owner.transform.position + offset;
		Vector3 endPos = owner.Agent.pathEndPosition + offset;
		float jumpHeight = owner.GetJumpHeight(dist);

		Vector3 curPos = startPos;
		Vector3 nextPos;
		int segment = 4;

		List<Vector3> posList = new List<Vector3>();
		float ratio;
		for (int i = 1; i <= segment; i++) 
		{
			ratio = (float) i / segment;
			nextPos = Vector3.Lerp(startPos, endPos, ratio);
			nextPos.y += jumpHeight * Mathf.Sin(ratio * 180f * Mathf.Deg2Rad);
			Vector3 velocity = nextPos - curPos;
			Vector3 dir = velocity.normalized;
			float mag = velocity.magnitude;

			posList.Add(curPos);
			posList.Add(nextPos);

			if (Physics.Raycast(curPos, dir, mag, LayerMask.GetMask("Default")) == true)
			{
				owner.LastAirLines = posList.ToArray();
				return false;
			}
			curPos = nextPos;
		}

		owner.LastAirLines = posList.ToArray();
		return true;
	}

	private bool CheckStone(float dist)
	{
		float arriveTime = dist / owner.StoneSpeed;
		Vector3 targetPos = owner.Agent.pathEndPosition;
		Vector3 ownerPosition = owner.transform.position;
		stoneVelocity = (targetPos - ownerPosition) / arriveTime;
		stoneVelocity.y = (targetPos.y - ownerPosition.y) / arriveTime
			+ (arriveTime * Physics.gravity.y) * 0.5f;

		int segment = 4;
		Vector3 curPos = ownerPosition;
		Vector3 nextPos;
		List<Vector3> posList = new List<Vector3>();
		for (int i = 1; i <= segment; i++)
		{
			float ratio = (float)i / segment; 
			nextPos = ownerPosition + stoneVelocity * ratio * arriveTime;

			Vector3 rayVel = nextPos - curPos;
			Vector3 dir = rayVel.normalized;
			float mag = rayVel.magnitude;

			posList.Add(curPos);
			posList.Add(nextPos);

			if (Physics.Raycast(curPos, dir, mag, LayerMask.GetMask("Default")) == true)
			{
				owner.LastAirLines = posList.ToArray();
				return false;
			}
			curPos = nextPos;
		}
		owner.LastAirLines = posList.ToArray();
		return true;
	}
}