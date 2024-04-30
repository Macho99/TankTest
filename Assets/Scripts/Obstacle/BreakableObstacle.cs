using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle), typeof(MeshRenderer), typeof(Collider))]
public abstract class BreakableObstacle : NetworkBehaviour
{
	[SerializeField] protected Collider[] cols;
	[SerializeField] protected MeshFilter meshFilter;
	[SerializeField] protected MeshRenderer meshRenderer;
	[SerializeField] protected NavMeshObstacle navObstacle;
	[SerializeField] NetworkObject netObj;

	[Networked, OnChangedRender(nameof(BreakRender))]
	public bool IsBreaked { get; protected set; }

	protected void BreakRender()
	{
		if (IsBreaked == true)
		{
			Break();
		}
	}

	protected virtual void Break()
	{
		navObstacle.enabled = false;
	}

	protected virtual void Awake()
	{
	}

	protected virtual void OnValidate()
	{
		if(netObj == null)
		{
			netObj = GetComponent<NetworkObject>();
			if(netObj == null)
			{
				netObj = gameObject.AddComponent<NetworkObject>();
			}
		}
		if (cols == null || cols.Length == 0)
			cols = GetComponents<Collider>();
		if (meshFilter == null)
			meshFilter = GetComponent<MeshFilter>();
		if (meshRenderer == null)
			meshRenderer = GetComponent<MeshRenderer>();
		if (navObstacle == null)
		{
			navObstacle = GetComponent<NavMeshObstacle>();
			navObstacle.carving = true;
		}


		if(meshFilter.sharedMesh.isReadable == false)
		{
			print($"{gameObject.name}의 Read/Write 속성을 켜세요");
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		IsBreaked = true;
	}

	private void OnTriggerStay(Collider other)
	{
		IsBreaked = true;
	}
}