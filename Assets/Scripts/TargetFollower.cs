using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollower : MonoBehaviour
{
	[SerializeField] Transform target;
	[SerializeField] Vector3 offset;
	[SerializeField] float speed = 2f;

	public void Init(Transform target, Vector3 offset, float speed)
	{
		this.target = target;
		this.offset = offset;
		this.speed = speed;
		transform.position = target.position + offset;
	}

	public virtual void Update()
	{
		ManualUpdate();
	}

	public void ManualUpdate()
	{
		if(target != null)
		{
			transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.deltaTime * speed);
		}
	}
}