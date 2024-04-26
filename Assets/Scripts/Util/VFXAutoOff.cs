using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXAutoOff : MonoBehaviour
{
	[SerializeField] float offTime = 1f;
	private float initOffTime; 

	private float curTime = 0f;


	protected virtual void Awake()
	{
		initOffTime = offTime;
	}

	protected void OnEnable()
	{
		_ = StartCoroutine(CoOff());
	}

	private IEnumerator CoOff()
	{
		while(true)
		{
			if(curTime > offTime)
			{
				GameManager.Resource.Destroy(gameObject);
			}
			curTime += Time.deltaTime;
			yield return null;
		}
	}

	protected virtual void OnDisable()
	{
		curTime = 0f;
		offTime = initOffTime;
		transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		transform.localScale = Vector3.one;
	}

	public void SetOffTime(float value)
	{
		offTime = value;
	}
}