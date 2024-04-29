using RayFire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class ShatterObstacle : BreakableObstacle
{
	[SerializeField] RayfireRigid rayfireRb;
	DemolitionType demolitionType = DemolitionType.AwakePrefragment;

	protected override void Break()
	{
		base.Break();
		foreach(MeshCollider col in meshCols)
		{
			col.enabled = false;
		}
		rayfireRb.Demolish();
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		if(rayfireRb == null)
		{
			rayfireRb = new GameObject(gameObject.name).AddComponent<RayfireRigid>();
			rayfireRb.AddComponent<MeshFilter>().mesh = meshFilter.sharedMesh;
			rayfireRb.AddComponent<MeshRenderer>().material = meshRenderer.sharedMaterial;
			meshRenderer.enabled = false;
			rayfireRb.transform.localScale = transform.localScale;
			rayfireRb.transform.SetParent(transform, false);
			rayfireRb.demolitionType = DemolitionType.AwakePrecache;
			rayfireRb.meshDemolition.totalAmount = 2;
			rayfireRb.limitations.col = false;
			rayfireRb.materials.iMat = GameManager.Resource.Load<Material>("Material/ShatterInerMat");
			rayfireRb.fading.lifeTime = 2f;
			rayfireRb.fading.fadeType = FadeType.ScaleDown;
			rayfireRb.demolitionType = demolitionType;
		}
	}
}