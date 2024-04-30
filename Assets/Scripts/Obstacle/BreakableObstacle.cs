using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle), typeof(MeshRenderer), typeof(Collider))]
public abstract class BreakableObstacle : MonoBehaviour
{
	[SerializeField] protected Collider[] childCols;
	[SerializeField] protected MeshFilter meshFilter;
	[SerializeField] protected MeshRenderer[] childRenderers;
	[SerializeField] protected MeshRenderer meshRenderer;
	[SerializeField] protected NavMeshObstacle navObstacle;
	[SerializeField] protected BreakableObjBehaviour owner;

	public int idx = -1;
	public bool IsBreaked { get; protected set; }

	protected virtual void Break(bool immediately = false)
	{
		navObstacle.enabled = false;
	}

	protected virtual void Awake()
	{
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
		BreakRequest();
	}

	private void OnTriggerStay(Collider other)
	{
		BreakRequest();
	}

	public void BreakRequest()
	{
		owner.BreakRequest(idx);
	}

	public void TryBreak(bool Immediatly = false)
	{
		if (IsBreaked == false)
		{
			IsBreaked = true;
			Break(Immediatly);
		}
	}
}