using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class PoolableDebrisRoot : DebrisRoot
{
	Vector3[] initChildPos;

	protected override void Awake()
	{
		base.Awake();
		initChildPos = new Vector3[childrenRb.Length];
		for(int i = 0; i < childrenRb.Length; i++)
		{
			initChildPos[i] = childrenRb[i].transform.localPosition;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		for(int i = 0; i < childrenRb.Length; i++)
		{
			childrenRb[i].transform.localScale = Vector3.one * curScale;
			childrenRb[i].transform.localRotation = Quaternion.identity;
			childrenRb[i].transform.localPosition = initChildPos[i];
		}
	}

	protected override void VirtualDestroy()
	{
		//base.VirtualDestroy();
		GameManager.Resource.Destroy(gameObject);
	}

	public void AddRandomVelocityAtPosition(Vector3 velocity, Vector3 position)
	{
		foreach (var child in childrenRb)
		{
			child.AddForceAtPosition(Random.value * velocity, position, ForceMode.VelocityChange);
		}
	}
}