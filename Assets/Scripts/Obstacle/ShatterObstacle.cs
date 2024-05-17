using Fusion;
using RayFire;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class ShatterObstacle : BreakableObstacle
{
	const float fadeWaitDuration = 5f;
	const float fadeDuration = 10f;
	public enum ColliderType { Mesh, Box }

	[SerializeField] public int fragmentCnt = 5;
	[SerializeField] public ColliderType colType = ColliderType.Mesh;
	[SerializeField] public string cacheName;
	[SerializeField] public DebrisRoot debrisRootPrefab;
	public const string cachePath = "Assets/Resources/RayfireCache";
	public const string resourcePath = "RayfireCache";

	public MeshFilter MeshFilter { get { return meshFilter; } }

	DebrisRoot debrisRoot;
	float curScale;

	protected override void OnValidate()
	{
		base.OnValidate();
		if(cacheName.IsNullOrEmpty())
		{
			cacheName = $"{meshFilter.sharedMesh.name}";
		}
		if (debrisRootPrefab == null)
		{
			debrisRootPrefab = Resources.Load<DebrisRoot>($"{resourcePath}/{cacheName}");
			if(debrisRootPrefab == null)
			{
				print($"{cacheName} �������� ĳ�����ּ���");
			}
		}
	}

	protected override void Break(bool immediately = false)
	{
		base.Break(immediately);
		foreach(MeshRenderer renderer in childRenderers)
		{
			renderer.enabled = false;
		}
		foreach(Collider col in childCols)
		{
			col.enabled = false;
		}

		if(immediately == false)
		{
			debrisRoot = Instantiate(debrisRootPrefab, transform.position, transform.rotation, transform);
			debrisRoot.Init(fadeWaitDuration, fadeDuration);
			debrisRoot.OnDestoyEvent += () => { debrisRoot = null; };
		}
	}

	public override void BreakEffect(BreakableObjBehaviour.BreakData breakData)
	{
		if(debrisRoot == null)
		{
			Debug.LogError("debrisRoot�� ��������� ���� BreakEffect�� ȣ���");
			return;
		}
		debrisRoot.AddExplosionForce(breakData.force,
			breakData.position, 5f);
	}
}
