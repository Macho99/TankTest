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
		if (owner.TargetData.IsTargeting == false)
		{
			if (owner.Agent.hasPath && owner.Agent.remainingDistance < owner.NormalAttackDist)
			{
				owner.Agent.ResetPath();
				ChangeState(BruteZombie.State.Idle);
				return true;
			}
			return false;
		}

		//플레이어가 좁은 건물로 들어갈 경우
		//if (owner.Agent.hasPath && owner.Agent.remainingDistance < 1f)
		if (owner.Agent.pathStatus == NavMeshPathStatus.PathPartial)
		{
			if (owner.TargetData.Distance > 5f)
			{
				ChangeState(BruteZombie.State.Roar);
				owner.TargetData.RemoveTarget();
				owner.PlayerFindTimer = TickTimer.CreateFromSeconds(owner.Runner, 10f);
				owner.Agent.ResetPath();
				return true;
			}
		}

		float attackDist = owner.NormalAttackDist;
		TargetData targetData = owner.TargetData;
		if (owner.IsBerserk)
		{
			//특수 공격 쿨타임 되면
			if (owner.TwoHandGroundTimer.ExpiredOrNotRunning(owner.Runner))
			{
				//특수 근접공격
				if (targetData.Distance < attackDist && targetData.Angle < 60f)
				{
					Attack(BruteZombie.AttackType.TwoHandGround);
					owner.TwoHandGroundTimer = TickTimer.CreateFromSeconds(owner.Runner, owner.TwoHandGroundCooltime);
					return true;
				}
			}
			if (owner.DashTimer.ExpiredOrNotRunning(owner.Runner))
			{
				if(CheckDash(targetData.Direction, targetData.Distance) == true)
				{
					//돌진 공격
					if (targetData.Distance < owner.DashDist && targetData.AbsAngle < 5f)
					{
						Attack(BruteZombie.AttackType.Dash);
						owner.DashTimer = TickTimer.CreateFromSeconds(owner.Runner, owner.DashCooltime);
						return true;
					}
				}
			}
			if (owner.JumpTimer.ExpiredOrNotRunning(owner.Runner))
			{
				//점프 공격
				if (owner.MinJumpDist < targetData.Distance && targetData.Distance < owner.MaxJumpDist)
				{
					if (CheckJump(targetData.Distance) == true)
					{
						Attack(BruteZombie.AttackType.Jump);
						owner.JumpTimer = TickTimer.CreateFromSeconds(owner.Runner, owner.JumpCooltime);
						return true;
					}
				}
			}
			if (owner.StoneTimer.ExpiredOrNotRunning(owner.Runner))
			{
				//돌 던지기 공격
				if (owner.MinStoneDist < targetData.Distance && targetData.Distance < owner.MaxStoneDist)
				{
					if (owner.CheckProjectile(owner.transform.position + Vector3.up * 3f, owner.Agent.pathEndPosition, 
						out stoneVelocity, 1.5f, owner.StoneSpeed, Physics.gravity, 
						LayerMask.GetMask("Default", "Breakable", "Environment")) == true)
					{
						Attack(BruteZombie.AttackType.ThrowStone);
						owner.StoneTimer = TickTimer.CreateFromSeconds(owner.Runner, owner.StoneCooltime);
						return true;
					}
				}
			}
		}
		if (targetData.Distance < owner.NormalAttackDist || owner.TargetData.CheckObstacleAttack(owner.transform.position) == true)
		{
			BruteZombie.AttackType attackType;
			if (targetData.Angle > 90f)
			{
				attackType = BruteZombie.AttackType.Back;
			}
            else if(targetData.Angle > 30f)
            {
				attackType = targetData.Sign < 0f ? BruteZombie.AttackType.LeftFoot : BruteZombie.AttackType.RightFoot;
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

	Vector3 lookDir;
	TickTimer stoneCreateTimer;
	TickTimer stoneThrowTimer;
	private void Attack(BruteZombie.AttackType type)
	{
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
			lookDir = owner.TargetData.Direction;
			animWait.startAction += () =>
			{
				stoneCreateTimer = TickTimer.CreateFromSeconds(owner.Runner, 1.2f);
				stoneThrowTimer = TickTimer.CreateFromSeconds(owner.Runner, 4.7f);
			};
			animWait.updateAction += () =>
			{
				owner.transform.rotation = Quaternion.RotateTowards(owner.transform.rotation,
					Quaternion.LookRotation(lookDir), 120f * owner.Runner.DeltaTime);

				if (stoneCreateTimer.Expired(owner.Runner))
				{
					owner.StoneActive = true;
					stoneCreateTimer = TickTimer.None;
				}

				if (stoneThrowTimer.Expired(owner.Runner))
				{
					owner.StoneActive = false;
					stoneThrowTimer = TickTimer.None;
					owner.Runner.Spawn(owner.StonePrefab, owner.StoneHolder.position, owner.StoneHolder.rotation,
						onBeforeSpawned: (runner, obj) =>
						{
							obj.GetComponent<BruteStone>().Init(owner.Object.Id, stoneVelocity);
						});
				}
			};
			animWait.exitAction += () =>
			{
				if (stoneThrowTimer.IsRunning)
				{
					owner.Runner.Spawn(owner.StonePrefab, owner.StoneHolder.position, owner.StoneHolder.rotation,
						onBeforeSpawned: (runner, obj) =>
						{
							obj.GetComponent<BruteStone>().Init(owner.Object.Id, -owner.transform.forward);
						});
				}
				stoneCreateTimer = TickTimer.None;
				stoneThrowTimer = TickTimer.None;
				owner.StoneActive = false;
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
				owner.LastJumpLines = posList.ToArray();
				return false;
			}
			curPos = nextPos;
		}

		owner.LastJumpLines = posList.ToArray();
		return true;
	}

	private bool CheckDash(Vector3 targetDir, float dist)
	{
		Vector3 offset = Vector3.up;
		return Physics.Raycast(owner.transform.position + offset,
			targetDir, dist, LayerMask.GetMask("Default")) == false;
	}

}