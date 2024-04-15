using System.Collections;
using UnityEngine;

public class ZombieRagdollExit : ZombieState
{

	
	public ZombieRagdollExit(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		owner.Anim.enabled = true;

		owner.SetRbKinematic(false);
		owner.SetAnimTrigger("Ragdoll");
		if (owner.Hips.up.y > 0f)
		{
			owner.SetAnimFloat("TurnDir", 1f);
		}
		else
		{
			owner.SetAnimFloat("TurnDir", 0f);
		}

		owner.Agent.enabled = false;
		Vector3 hipPos = owner.Hips.localPosition;

		ZombieHitBox leftKneeBox = owner.BodyParts[(int)ZombieBody.LeftKnee].zombieHitBox;
		Vector3 leftFootPos = leftKneeBox.transform.position + leftKneeBox.transform.right * leftKneeBox.BoxExtents.x * 2f;
		ZombieHitBox rightKneeBox = owner.BodyParts[(int)ZombieBody.RightKnee].zombieHitBox;
		Vector3 rightFootPos = rightKneeBox.transform.position - rightKneeBox.transform.right * rightKneeBox.BoxExtents.x * 2f;

		owner.transform.position = (leftFootPos + rightFootPos) * 0.5f;
		GameObject obj = new GameObject("Pos");
		obj.transform.position = owner.transform.position;
		owner.Hips.localPosition = hipPos;
		owner.Agent.enabled = true;
	}

	public override void Exit()
	{
	}

	public override void FixedUpdateNetwork()
	{
	}

	public override void Render()
	{
		
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{

	}
}