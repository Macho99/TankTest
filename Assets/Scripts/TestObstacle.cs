using RayFire;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(RayfireRigid))]
public class TestObstacle : MonoBehaviour
{
	public enum ColliderType { Mesh, Box }

	[SerializeField] public int fragmentCnt = 5;
	[SerializeField] public ColliderType colType = ColliderType.Mesh;
	[SerializeField] public RayfireRigid rayfireRb;
	[SerializeField] public MeshFilter meshFilter;
	[SerializeField] public string cacheName;
	public const string cachePath = "Assets/Resources/RayfireCache";
	public const string resourcePath = "RayfireCache";

	bool isbreaked = false;

	private void Awake()
	{
		if(rayfireRb.referenceDemolition.reference == null)
		{
			rayfireRb.referenceDemolition.reference = Resources.Load<GameObject>(
				$"{resourcePath}/{cacheName}");
		}
	}

	private void OnValidate()
	{
		if(meshFilter == null)
		{
			meshFilter = GetComponent<MeshFilter>();
			cacheName = $"{meshFilter.sharedMesh.name}";
		}
		if (rayfireRb == null)
		{
			rayfireRb = GetComponent<RayfireRigid>();
			rayfireRb.demolitionType = DemolitionType.ReferenceDemolition;
			rayfireRb.fading.fadeType = FadeType.ScaleDown;
			rayfireRb.fading.lifeTime = 1f;
		}
		if (rayfireRb.referenceDemolition.reference == null)
		{
			rayfireRb.referenceDemolition.reference = Resources.Load<GameObject>($"{resourcePath}/{cacheName}");
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if(isbreaked == false)
		{
			isbreaked = true;
			rayfireRb.Demolish();
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (isbreaked == false)
		{
			isbreaked = true;
			rayfireRb.Demolish();
		}
	}
}
