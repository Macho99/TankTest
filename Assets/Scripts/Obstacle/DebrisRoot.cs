using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisRoot : MonoBehaviour
{
	[SerializeField] Rigidbody[] childrenRb;

	public Rigidbody[] ChildrenRb { get { return childrenRb; } }

	private void OnValidate()
	{
		InitChildren();
	}

	public void InitChildren()
	{
		if (childrenRb == null || childrenRb.Length == 0)
		{
			childrenRb = GetComponentsInChildren<Rigidbody>();
		}
	}

	public void SetChildrenScale(float scale)
	{
		foreach (var child in childrenRb)
		{
			child.transform.localScale = Vector3.one * scale;
		}
	}

	public void AddExplosionForce(float force, Vector3 position, float radius)
	{
		foreach(var child in childrenRb)
		{
			child.AddExplosionForce(force, position, radius);
		}
	}

	public void AddForceAtPosition(Vector3 force, Vector3 position)
	{
		foreach(var child in childrenRb)
		{
			child.AddForceAtPosition(force, position, ForceMode.Impulse);
		}
	}
}
