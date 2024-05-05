using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class TargetData
{
	private Transform owner;
	private NetworkRunner runner;
	public bool IsTargeting { get { return Transform != null; } }
	public Transform Transform { get; private set; }
	public Vector3 Position { get { return Transform.position; } }
	public int Layer { get; private set; }

	public Tick LastFindTick { get; set; }

	public Vector3 Direction { get; private set; }
	public float Distance { get; private set; }
	public float AbsAngle { get; private set; }
	public float Angle { get; private set; }
	public float Sign { get; private set; }

	private RaycastHit[] targetHits;
	private RaycastHit[] ownerHits;

	private LayerMask obstacleMask;

	public TargetData(Transform owner, NetworkRunner runner)
	{
		this.owner = owner;
		this.runner = runner;

		targetHits = new RaycastHit[5];
		ownerHits = new RaycastHit[5];

		obstacleMask = LayerMask.GetMask("Breakable", "Vehicle");
	}

	public void SetTarget(Transform target = null)
	{
		if (target == null)
		{
			Transform = null;
			Layer = -1;
		}
		else
		{
			Transform = target;
			Layer = target.gameObject.layer;
			LastFindTick = runner.Tick;
			UpdateTargetData();
		}
	}

	public void UpdateTargetData()
	{
		if (IsTargeting == false) return;

		Vector3 diff = Position - owner.transform.position;
		Distance = diff.magnitude;
		diff.y = 0f;
		Direction = diff.normalized;

		Angle = Vector3.SignedAngle(owner.transform.forward, Direction, owner.transform.up);
		Sign = (Angle >= 0f) ? 1f : -1f;

		AbsAngle = Mathf.Abs(Angle);
	}

	public bool CheckObstacleAttack(Vector3 ownerPosition)
	{
		if (Distance > 7f)
			return false;

		Vector3 obstacleDir = ownerPosition - Transform.position;
		obstacleDir.Normalize();

		Vector3 offset = Vector3.up * 0.1f;

		int ownerResult = Physics.RaycastNonAlloc(ownerPosition + offset, -obstacleDir, ownerHits, 1f, obstacleMask);
		if (ownerResult == 0) return false;

		int targetResult = Physics.RaycastNonAlloc(Position + offset, obstacleDir, targetHits, 1f, obstacleMask);
		if (targetResult == 0)
		{
			//차량에 타고 있을 때 예외처리
			targetResult = Physics.RaycastNonAlloc(Position + Vector3.up * 5f, Vector3.down, targetHits, 5f, obstacleMask);
		}
		if (targetResult == 0) return false;

		for (int i = 0; i < targetResult; i++)
		{
			int idx = Array.FindIndex(ownerHits, 0, ownerResult, 
				(other) => { return targetHits[i].collider.gameObject == other.collider.gameObject; });
			if(idx != -1) 
				return true;
		}
		
		return false;
	}
}