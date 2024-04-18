using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetecter : MonoBehaviour
{
	Zombie owner;

	private void Awake()
	{
		owner = GetComponentInParent<Zombie>();
	}

	private void OnTriggerStay(Collider other)
	{
		//owner.OnTargetTriggerStay(other.transform);
	}
}
