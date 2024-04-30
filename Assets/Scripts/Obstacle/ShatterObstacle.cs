using Fusion;
using RayFire;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class ShatterObstacle : BreakableObstacle
{
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

	public override void FixedUpdateNetwork()
	{
		base.FixedUpdateNetwork();
		if (debrisFadeTimer.IsRunning)
		{
			if (debrisFadeTimer.Expired(Runner))
			{
				Destroy(debrisTransList[0].gameObject);
				debrisTransList.Clear();
				debrisFadeTimer = TickTimer.None;
			}
			else
			{
				float curScale = debrisTransList[1].localScale.x;
				float nextScale = Mathf.Max(curScale - Runner.DeltaTime / fadeDuration, 0f);
				for (int i = 1; i < debrisTransList.Count; i++)
				{
					debrisTransList[i].localScale = Vector3.one * nextScale;
				}
			}
		}
	}

	protected override void Break()
	{
		base.Break();
		meshRenderer.enabled = false;
		foreach(Collider col in cols)
		{
			col.enabled = false;
		}
		GameObject debris = Instantiate(debrisPrefab, transform.position, transform.rotation, transform);
		debrisTransList.Add(debris.transform);
		foreach(Transform child in debris.transform)
		{
			debrisTransList.Add(child);
		}
		debrisFadeTimer = TickTimer.CreateFromSeconds(Runner, fadeDuration);
	}
}
