using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum FallType { Inplace, ForwardLow, ForwardMid }

[RequireComponent(typeof(Collider))]
public class FallDownableObj : MonoBehaviour
{
	[SerializeField] FallType falldownType = FallType.ForwardLow;

	Collider col;
	private void Awake()
	{
		col = GetComponent<Collider>();
		col.isTrigger = true;

		gameObject.layer = LayerMask.NameToLayer("IgnorePlayer");

		GameObject child = new GameObject("IgnoreMonster");
		child.transform.SetParent(transform, false);
		child.layer = LayerMask.NameToLayer("IgnoreMonster");
		if (TryGetComponent(out MeshCollider meshCol))
		{
			MeshCollider childMesh = child.AddComponent<MeshCollider>();
			childMesh.convex = meshCol.convex;
			childMesh.sharedMesh = meshCol.sharedMesh;
		}
		else
		{
			print("다른 콜라이더용 코드 추가");
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if(TryGetComponent(out Zombie zombie))
		{
			//zombie.Falldown(falldownType);
		}
	}
}
