using System;
using System.Collections;
using UnityEngine;

public class ZombieRagdollExit : ZombieState
{
	const float resetBoneTime = 0.5f;
	float elapsed;
	BoneTransform[] faceBoneTransforms;
	string animName;

	public ZombieRagdollExit(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		elapsed = 0f;

		owner.SetRbKinematic(true);
		owner.Agent.enabled = false;

		float sign;
		if (owner.CurRagdollState == RagdollState.FaceUpStand)
		{
			sign = -1f;
			owner.SetAnimFloat("TurnDir", 1f);
			faceBoneTransforms = Zombie.BoneTransDict["FaceUpStand".GetHashCode()];
			animName = "RagdollToStand";
		}
		else if(owner.CurRagdollState == RagdollState.FaceDownStand)
		{
			sign = 1f;
			owner.SetAnimFloat("TurnDir", 0f);
			faceBoneTransforms = Zombie.BoneTransDict["FaceDownStand".GetHashCode()];
			animName = "RagdollToStand";
		}
		else if(owner.CurRagdollState == RagdollState.FaceUpCrawl)
		{
			sign = 1f;
			owner.SetAnimFloat("TurnDir", 1f);
			faceBoneTransforms = Zombie.BoneTransDict["FaceUpCrawl".GetHashCode()];
			animName = "RagdollToCrawl";
		}
		else if(owner.CurRagdollState == RagdollState.FaceDownCrawl)
		{
			sign = 1f;
			owner.SetAnimFloat("TurnDir", 0f);
			faceBoneTransforms = Zombie.BoneTransDict["FaceDownCrawl".GetHashCode()];
			animName = "RagdollToCrawl";
		}
		else
		{
			Debug.LogError($"{owner.CurRagdollState}를 확인하세요");
			return;
		}

		owner.Anim.Play(animName, 0, 0f);
		AlignRotationToHips(sign);
		AlignPositionToHips();
		owner.Agent.enabled = true;

		//Vector3 hipPos = owner.Hips.localPosition;
		//ZombieHitBox leftKneeBox = owner.BodyParts[(int)ZombieBody.LeftKnee].zombieHitBox;
		//Vector3 leftFootPos = leftKneeBox.transform.position + leftKneeBox.transform.right * leftKneeBox.BoxExtents.x * 2f;
		//ZombieHitBox rightKneeBox = owner.BodyParts[(int)ZombieBody.RightKnee].zombieHitBox;
		//Vector3 rightFootPos = rightKneeBox.transform.position - rightKneeBox.transform.right * rightKneeBox.BoxExtents.x * 2f;

		//owner.transform.position = (leftFootPos + rightFootPos) * 0.5f;
		//owner.LastFootPos = owner.transform.position;
		//owner.Hips.localPosition = hipPos;

		owner.CopyBoneTransforms(owner.RagdollBoneTransforms);
	}

	private void AlignRotationToHips(float sign)
	{
		Vector3 prevHipPos = owner.Hips.position;
		Quaternion prevHipRot = owner.Hips.rotation;

		Vector3 hipDir = -owner.Hips.transform.right;
		hipDir.y = 0f;

		owner.transform.rotation = Quaternion.LookRotation(hipDir * sign);

		owner.Hips.position = prevHipPos;
		owner.Hips.rotation = prevHipRot;
	}

	private void AlignPositionToHips()
	{
		Vector3 prevHipPos = owner.Hips.position;
		Vector3 newRootPos = prevHipPos;

		Vector3 offset = faceBoneTransforms[0].localPosition;
		offset.y = 0f;
		offset = owner.transform.rotation * offset;

		if (Physics.Raycast(prevHipPos, Vector3.down, out RaycastHit hitInfo))
		{
			newRootPos = hitInfo.point;
		}
		owner.transform.position = newRootPos - offset;
		
		owner.Hips.position = prevHipPos;
	}

	public override void Exit()
	{
		owner.Anim.enabled = true;
	}

	public override void FixedUpdateNetwork()
	{
	}

	public override void Render()
	{
		elapsed += Time.deltaTime;

		float ratio = elapsed / resetBoneTime;
		ratio = Mathf.Clamp01(ratio);
		ratio = BezierBlend(ratio);

		owner.transform.position = Vector3.Lerp(owner.transform.position, owner.Position, ratio);
		owner.transform.rotation = Quaternion.Lerp(owner.transform.rotation, owner.Rotation, ratio);

		for (int i = 0; i < owner.Bones.Length; i++)
		{
			owner.Bones[i].localPosition = Vector3.Lerp(
				owner.RagdollBoneTransforms[i].localPosition,
				faceBoneTransforms[i].localPosition,
				ratio);

			owner.Bones[i].localRotation = Quaternion.Lerp(
				owner.RagdollBoneTransforms[i].localRotation,
				faceBoneTransforms[i].localRotation,
				ratio);
		}

		if(elapsed > resetBoneTime)
		{
			owner.AnimWaitStruct = new AnimWaitStruct(animName, 
				animEndAction: () => owner.CurRagdollState = RagdollState.Animate);
			ChangeState(Zombie.State.AnimWait);
			return;
		}
	}

	public override void SetUp()
	{

	}

	public override void Transition()
	{

	}
	private float BezierBlend(float t)
	{
		return t * t * (3.0f - 2.0f * t);
	}
}