using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXAutoOff : MonoBehaviour
{
	[SerializeField] protected float offTime = 1f;
	private float initOffTime; 

	protected float elapsed = 0f;

	protected virtual void Awake()
	{
		initOffTime = offTime;
	}

	protected virtual void OnEnable()
	{
		elapsed = 0f;
	}

	protected virtual void Update()
	{
		if (elapsed > offTime)
		{
			GameManager.Resource.Destroy(gameObject);
		}
		elapsed += Time.deltaTime;
	}

	protected virtual void OnDisable()
	{
		offTime = initOffTime;
		transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		transform.localScale = Vector3.one;
	}

	public void AutoSetParentNull(float nullTime)
	{
		StartCoroutine(CoAutoSetParent(nullTime));
	}

	private IEnumerator CoAutoSetParent(float nullTime)
	{
		yield return new WaitForSeconds(nullTime);
		transform.parent = null;
	}

	public void SetOfftimeWithElapsed(float value)
	{
		offTime = value + elapsed;
	}

	public void SetOffTime(float value)
	{
		offTime = value;
	}
}