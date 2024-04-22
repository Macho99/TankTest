using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InGameUI : BaseUI
{
	protected Transform followTarget;
	protected Vector3 followOffset;

	protected virtual void LateUpdate()
	{
		if (followTarget != null)
		{
			transform.position = Camera.main.WorldToScreenPoint(followTarget.position) + followOffset;
		}
	}

	public void SetTarget(Transform target)
	{
		followTarget = target;
		Init();
	}

	protected abstract void Init();

	public void SetOffset(Vector3 offset)
	{
		followOffset = offset;
		if (followTarget != null)
		{
			transform.position = Camera.main.WorldToScreenPoint(followTarget.position) + followOffset;
		}
	}

	public override void CloseUI()
	{
		GameManager.UI.CloseInGameUI(this);
	}
}