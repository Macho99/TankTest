using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SingleBreakObstacle : BreakableObstacle
{
	const float fadeWaitDuration = 10f;
	const float fadeDuration = 5f;
	Rigidbody childRb;

	private IEnumerator CoFade()
	{
		yield return new WaitForSeconds(fadeWaitDuration);

		while (true)
		{
			float curScale = childRb.transform.localScale.x;
			float nextScale = curScale - Time.deltaTime / fadeDuration;
			if (nextScale < 0)
			{
				break;
			}


			childRb.transform.localScale = Vector3.one * nextScale;
			yield return null;
		}

		Destroy(childRb);
		childRb = null;
	}

	protected override void Break(bool immediately = false)
	{
		base.Break(immediately);
		foreach (MeshRenderer renderer in childRenderers)
		{
			renderer.enabled = false;
		}
		foreach (Collider col in childCols)
		{
			col.enabled = false;
		}

		if (immediately == false)
		{
			GameObject childObj = new GameObject("BreakVisual");
			childObj.transform.SetParent(transform, false);
			childObj.AddComponent<MeshFilter>().sharedMesh = meshFilter.sharedMesh;
			childObj.AddComponent<MeshRenderer>().material = meshRenderer.material;
			MeshCollider meshCol = childObj.AddComponent<MeshCollider>();
			meshCol.sharedMesh = meshFilter.sharedMesh;
			meshCol.convex = true;
			childRb = childObj.AddComponent<Rigidbody>();
			childRb.mass = 20f;

			StartCoroutine(CoFade());
		}
	}

	public override void BreakEffect(BreakableObjBehaviour.BreakData breakData)
	{
		if (childRb == null)
		{
			Debug.LogError("child가 만들어지기 전에 BreakEffect가 호출됨");
			return;
		}

		if (breakData.type == BreakableObjBehaviour.BreakType.AddForce)
		{
			childRb.AddForceAtPosition(breakData.velocityOrForceAndRadius, breakData.position);
		}
		else if (breakData.type == BreakableObjBehaviour.BreakType.Explosion)
		{
			childRb.AddExplosionForce(breakData.velocityOrForceAndRadius.x,
				breakData.position, breakData.velocityOrForceAndRadius.y);
		}
	}
}