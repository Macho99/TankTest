using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ZombieRagdollEnter : ZombieState
{
	float elapsed;
	float exitTime;

	bool transition;

	public ZombieRagdollEnter(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		owner.Agent.enabled = false;
		transition = false;
		elapsed = 0f;
		exitTime = 0.3f;

		if(owner.CurHp > 0)
		{
			owner.transform.position = owner.Position;
			owner.transform.rotation = owner.Rotation;
		}

		owner.Anim.enabled = false;
		//BoneTransform[] boneTransforms = Zombie.BoneTransDict[owner.Anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.GetHashCode()];
		//for (int i = 0; i < owner.Bones.Length; i++)
		//{
		//	owner.Bones[i].localPosition = boneTransforms[i].localPosition;
		//	owner.Bones[i].localRotation = boneTransforms[i].localRotation;
		//}
		owner.EnableRagdoll(true);
		Rigidbody rb = owner.BodyHitParts[(int)owner.HitBody].rb;
		rb.AddForce(owner.HitVelocity * rb.mass, ForceMode.Impulse);
	}

	public override void Exit()
	{
	}

	public override void FixedUpdateNetwork()
	{
		elapsed += owner.Runner.DeltaTime;
		if(transition == false)
			owner.CurRagdollState = RagdollState.Ragdoll;
		owner.Decelerate(1f);
	}

	public override void Render()
	{
		owner.Agent.enabled = false;

		for(int i = 0; i < owner.Bones.Length; i++)
		{
			owner.Bones[i].localPosition = owner.RagdollBones[i].localPosition;
			owner.Bones[i].localRotation = owner.RagdollBones[i].localRotation;
		}

		Vector3 prevHipPos = owner.Hips.position;
		Vector3 newRootPos = prevHipPos;

		if (Physics.Raycast(prevHipPos, Vector3.down, out RaycastHit hitInfo, 5f, LayerMask.GetMask("Default")))
		{
			newRootPos = hitInfo.point;
		}

		owner.transform.position = newRootPos;

		owner.Hips.position = prevHipPos;
		owner.RagdollHips.position = prevHipPos;
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		if (transition == true) return;

		if(elapsed > exitTime)
		{
			if(owner.CurHp <= 0)
			{
				ChangeState(Zombie.State.Die);
				return;
			}

			bool crawl = owner.Anim.GetBool("Crawl");

			if (owner.Hips.up.y > 0f)
			{
				if (owner.CurLegHp > 0 && crawl == false)
					owner.CurRagdollState = RagdollState.FaceUpStand;
				else
					owner.CurRagdollState = RagdollState.FaceUpCrawl;
			}
			else
			{
				if (owner.CurLegHp > 0 && crawl == false)
					owner.CurRagdollState = RagdollState.FaceDownStand;
				else
					owner.CurRagdollState = RagdollState.FaceDownCrawl;
			}
			transition = true;
		}
	}
}