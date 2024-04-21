using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ZombieDie : ZombieState
{
	const float despawnTime = 3f;
	float elapsed;

	public ZombieDie(Zombie owner) : base(owner)
	{
	}

	public override void Enter()
	{
		elapsed = 0f;
	}

	public override void Exit()
	{

	}

	public override void FixedUpdateNetwork()
	{
		elapsed += owner.Runner.DeltaTime;
		
		if(elapsed > despawnTime)
		{
			owner.Runner.Despawn(owner.Object);
			ChangeState(Zombie.State.Wait);
			return;
		}


		owner.Agent.enabled = false;

		for (int i = 0; i < owner.Bones.Length; i++)
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

	}
}