using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ZombieDie : ZombieBaseDie
{
	new Zombie owner;

	public ZombieDie(Zombie owner) : base(owner, 3f)
	{
		this.owner = owner;
	}

	public override void FixedUpdateNetwork()
	{
		base.FixedUpdateNetwork();

		if (despawnTimer.IsRunning == false) return;

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
}