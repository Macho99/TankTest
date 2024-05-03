using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle), typeof(MeshRenderer), typeof(Collider))]
public abstract class BreakableObstacle : MonoBehaviour, IHittable
{
	[SerializeField] protected Collider[] childCols;
	[SerializeField] protected MeshFilter meshFilter;
	[SerializeField] protected MeshRenderer[] childRenderers;
	[SerializeField] protected MeshRenderer meshRenderer;
	[SerializeField] protected NavMeshObstacle navObstacle;
	[SerializeField] protected BreakableObjBehaviour owner;

	protected LayerMask breakMask;

	public int idx = -1;
	public bool IsBreaked { get; protected set; }
	public int CurHp { get; protected set; } = 100;

	public uint HitID => owner.Object.Id.Raw * (uint) idx;

	protected virtual void Break(bool immediately = false)
	{
		navObstacle.enabled = false;
	}

	protected virtual void Awake()
	{
		breakMask = LayerMask.GetMask("Default", "Vehicle");
	}

	protected virtual void OnValidate()
	{
		if (owner == null)
			owner = GetComponentInParent<BreakableObjBehaviour>();
		if(idx == -1)
		{
			idx = owner.RegisterObj(this);
		}
		if (childCols == null || childCols.Length == 0)
			childCols = GetComponentsInChildren<Collider>();
		if (meshFilter == null)
			meshFilter = GetComponent<MeshFilter>();
		if (meshRenderer == null)
			meshRenderer = GetComponent<MeshRenderer>();
		if (navObstacle == null)
		{
			navObstacle = GetComponent<NavMeshObstacle>();
			navObstacle.carving = true;
		}
		if(childRenderers == null || childRenderers.Length == 0)
			childRenderers = GetComponentsInChildren<MeshRenderer>();
	}

	private void OnCollisionStay(Collision collision)
	{
		if(breakMask.IsLayerInMask(collision.collider.gameObject.layer) == true)
		{
			BreakRequest();
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (breakMask.IsLayerInMask(other.gameObject.layer) == true)
		{
			BreakRequest();
		}
	}

	private void BreakRequest()
	{
		owner.BreakRequest(idx);
	}

	private void ExplosionBreakRequest(float force, Vector3 position)
	{
		BreakableObjBehaviour.BreakData breakData = new BreakableObjBehaviour.BreakData()
		{
			idx = this.idx,
			position = position,
			force = force
		};
		owner.BreakRequest(breakData);
	}

	//public void AddForceBreakRequest(Vector3 force, Vector3 position)
	//{
	//	BreakableObjBehaviour.BreakData breakData = new BreakableObjBehaviour.BreakData()
	//	{
	//		idx = this.idx,
	//		position = position,
	//		type = BreakableObjBehaviour.BreakType.AddForce,
	//		velocityOrForceAndRadius = force
	//	};
	//	owner.BreakRequest(breakData);
	//}

	public void OwnerTryBreak(bool Immediatly = false)
	{
		if (IsBreaked == false)
		{
			IsBreaked = true;
			Break(Immediatly);
		}
	}

	public abstract void BreakEffect(BreakableObjBehaviour.BreakData breakData);

	public void ApplyDamage(Transform source, Vector3 point, Vector3 force, int damage)
	{
		CurHp -= damage;
		if(CurHp <= 0)
		{
			ExplosionBreakRequest(force.magnitude, point);
		}
	}
}