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
	GameObject child;

	private IEnumerator CoFade()
	{
		yield return new WaitForSeconds(fadeWaitDuration);

		while (true)
		{
			float curScale = child.transform.localScale.x;
			float nextScale = curScale - Time.deltaTime / fadeDuration;
			if (nextScale < 0)
			{
				break;
			}


			child.transform.localScale = Vector3.one * nextScale;
			yield return null;
		}

		Destroy(child);
		child = null;
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
			child = new GameObject("BreakVisual");
			child.transform.SetParent(transform, false);
			child.AddComponent<MeshFilter>().sharedMesh = meshFilter.sharedMesh;
			child.AddComponent<MeshRenderer>().material = meshRenderer.material;
			MeshCollider meshCol = child.AddComponent<MeshCollider>();
			meshCol.sharedMesh = meshFilter.sharedMesh;
			meshCol.convex = true;
			child.AddComponent<Rigidbody>().mass = 20f;

			StartCoroutine(CoFade());
		}
	}
}