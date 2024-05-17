using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DebrisRoot : MonoBehaviour
{
	[SerializeField] protected Rigidbody[] childrenRb;
	[SerializeField] float fadeWaitDuration = 5f;
	[SerializeField] float fadeDuration = 10f;

	protected float curScale = 0.9f;

	public event Action OnDestoyEvent;
	public Rigidbody[] ChildrenRb { get { return childrenRb; } }

	protected virtual void Awake()
	{

	}

	protected virtual void OnEnable()
	{
		StartCoroutine(CoEnable());
	}

	protected virtual void OnDisable()
	{
		curScale = 0.9f;
		OnDestoyEvent = null;
	}

	private void OnValidate()
	{
		InitChildren();
	}

	public void Init(float fadeWaitDuration, float fadeDuration)
	{
		this.fadeWaitDuration = fadeWaitDuration;
		this.fadeDuration = fadeDuration;
	}

	private IEnumerator CoEnable()
	{
		yield return null;
		yield return new WaitForSeconds(fadeWaitDuration);
		curScale = 0.9f;

		while (true)
		{
			float nextScale = curScale - Time.deltaTime / fadeDuration;
			if (nextScale < 0)
			{
				break;
			}

			SetChildrenScale(nextScale);
			curScale = nextScale;
			yield return null;
		}

		OnDestoyEvent?.Invoke();
		VirtualDestroy();
	}

	protected virtual void VirtualDestroy()
	{
		Destroy(gameObject);
	}

	public void InitChildren()
	{
		if (childrenRb == null || childrenRb.Length == 0)
		{
			childrenRb = GetComponentsInChildren<Rigidbody>();
		}
	}

	private void SetChildrenScale(float scale)
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
