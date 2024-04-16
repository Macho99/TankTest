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

	public ZombieRagdollEnter(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		elapsed = 0f;
		exitTime = 20f;

		owner.Anim.enabled = false;
		BoneTransform[] boneTransforms = Zombie.BoneTransDict[owner.Anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.GetHashCode()];
		for (int i = 0; i < owner.Bones.Length; i++)
		{
			owner.Bones[i].localPosition = boneTransforms[i].localPosition;
			owner.Bones[i].localRotation = boneTransforms[i].localRotation;
		}
		//owner.BodyParts[(int)owner.RagdollBody].rb.AddForce(owner.RagdollVelocity, ForceMode.Impulse);
		owner.SetRbKinematic(false);
	}

	public override void Exit()
	{
	}

	public override void FixedUpdateNetwork()
	{
		elapsed += owner.Runner.DeltaTime;
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{
		if(elapsed > exitTime)
		{
			owner.IsRagdoll = false;
		}
	}
}