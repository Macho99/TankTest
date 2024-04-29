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
	[SerializeField] DemolitionType demolitionType = DemolitionType.Runtime;

	protected override void Break()
	{
		base.Break();
		RayfireRigid rayfire = new GameObject(gameObject.name).AddComponent<RayfireRigid>();
		rayfire.AddComponent<MeshFilter>().mesh = meshFilter.sharedMesh;
		rayfire.AddComponent<MeshRenderer>().material = meshRenderer.sharedMaterial;
		meshRenderer.enabled = false;
		foreach(MeshCollider col in meshCols)
		{
			col.enabled = false;
		}
		rayfire.transform.position = transform.position;
		rayfire.transform.rotation = transform.rotation;
		rayfire.transform.localScale = transform.localScale;
		rayfire.demolitionType = demolitionType;
		rayfire.meshDemolition.totalAmount = 2;
		rayfire.limitations.col = false;
		rayfire.materials.iMat = GameManager.Resource.Load<Material>("Material/ShatterInerMat");
		rayfire.fading.fadeType = FadeType.ScaleDown;
		rayfire.fading.lifeTime = 2f;
		rayfire.Demolish();
	}
}