using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VisualEffectOff : MonoBehaviour
{
	VisualEffect visualEffect;

	private void Awake()
	{
		visualEffect = GetComponentInChildren<VisualEffect>();
	}

	public void Init(float offTime, float destroyTime)
	{
		StartCoroutine(CoOff(offTime, destroyTime));
	}

	private IEnumerator CoOff(float offTime, float destroyTime)
	{
		yield return new WaitForSeconds(offTime);
		visualEffect.Stop();
		yield return new WaitForSeconds(destroyTime);
		GameManager.Resource.Destroy(gameObject);
		Destroy(this);
	}
}