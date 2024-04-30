using Fusion;
using RayFire;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class ShatterObstacle : BreakableObstacle
{
	const float fadeWaitDuration = 3f;
	const float fadeDuration = 5f;
	public enum ColliderType { Mesh, Box }

	[SerializeField] public int fragmentCnt = 5;
	[SerializeField] public ColliderType colType = ColliderType.Mesh;
	[SerializeField] public string cacheName;
	[SerializeField] public GameObject debrisPrefab;
	public const string cachePath = "Assets/Resources/RayfireCache";
	public const string resourcePath = "RayfireCache";

	public MeshFilter MeshFilter { get { return meshFilter; } }

	List<Transform> debrisTransList = new();
	TickTimer debrisFadeTimer;

	protected override void OnValidate()
	{
		base.OnValidate();
		if(cacheName.IsNullOrEmpty())
		{
			cacheName = $"{meshFilter.sharedMesh.name}";
		}
		if (debrisPrefab == null)
		{
			debrisPrefab = Resources.Load<GameObject>($"{resourcePath}/{cacheName}");
			if(debrisPrefab == null)
			{
				print($"{cacheName} 프리팹을 캐시해주세요");
			}
		}
	}

	private IEnumerator CoFade()
	{
		yield return new WaitForSeconds(fadeWaitDuration);

		while (true)
		{
			float curScale = debrisTransList[1].localScale.x;
			float nextScale = curScale - Time.deltaTime / fadeDuration;
			if(nextScale < 0)
			{
				break;
			}

			for (int i = 1; i < debrisTransList.Count; i++)
			{
				debrisTransList[i].localScale = Vector3.one * nextScale;
			}
			yield return null;
		}

		Destroy(debrisTransList[0].gameObject);
		debrisTransList.Clear();
		debrisFadeTimer = TickTimer.None;
	}

	protected override void Break(bool immediately = false)
	{
		base.Break(immediately);
		meshRenderer.enabled = false;
		foreach(Collider col in cols)
		{
			col.enabled = false;
		}

		if(immediately == false)
		{
			GameObject debris = Instantiate(debrisPrefab, transform.position, transform.rotation, transform);
			debrisTransList.Add(debris.transform);
			foreach (Transform child in debris.transform)
			{
				debrisTransList.Add(child);
			}
			StartCoroutine(CoFade());
		}
	}
}
