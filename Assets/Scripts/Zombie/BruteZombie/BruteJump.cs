using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BruteJump : BruteZombieState
{
	const float jumpStartRatio = 0.34f;
	const float jumpEndRatio = 0.586f;

	float jumpHeight;
	Vector3 startPos;
	Vector3 endPos;
	Vector3 lookDir;

	bool animEntered;

	public BruteJump(BruteZombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		owner.SetAnimTrigger("Attack");
		owner.SetAnimFloat("ActionShifter", (float) BruteZombie.AttackType.Jump);

		animEntered = false;

		owner.LookWeight = 0f;
		owner.SyncTransfrom = false;
		startPos = owner.Position;
		endPos = owner.JumpEndPos;

		lookDir = endPos - startPos;
		jumpHeight = owner.GetJumpHeight(lookDir.magnitude);

		lookDir.y = 0f;
		lookDir.Normalize();

		owner.Agent.enabled = false;
	}

	public override void Exit()
	{
		owner.LookWeight = 1f;
		owner.SyncTransfrom = true;
		owner.Agent.enabled = true;
	}

	public override void FixedUpdateNetwork()
	{
		owner.Decelerate();
	}

	public override void Render()
	{
		owner.transform.rotation = Quaternion.RotateTowards(owner.transform.rotation,
			Quaternion.LookRotation(lookDir), 120f * Time.deltaTime);

		if (owner.IsAnimName("Attack"))
		{
			animEntered = true;
			Move(owner.GetAnimNormalTime());
		}

		if(animEntered == true && owner.IsAnimName("Attack") == false)
		{
			ChangeState(BruteZombie.State.Trace);
		}
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{

	}

	private void Move(float animRatio)
	{
		animRatio = Mathf.Clamp(animRatio, jumpStartRatio, jumpEndRatio);
		animRatio = (animRatio - jumpStartRatio) / (jumpEndRatio - jumpStartRatio);
		animRatio = Mathf.Clamp01(animRatio);

		Vector3 pos = Vector3.Lerp(startPos, endPos, animRatio);
		pos.y += jumpHeight * Mathf.Sin(animRatio * 180f * Mathf.Deg2Rad);
		owner.transform.position = pos;
	}
}