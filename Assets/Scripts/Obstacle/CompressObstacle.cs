using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CompressObstacle : BreakableObstacle
{
	const string smokeVfxPath = "FX/VFX/CompressSmoke";

	[SerializeField] float scaleYValue;
	Mesh mesh;
	Vector3[] vertices;
	int environLayer;

	protected override void Awake()
	{
		base.Awake();
		environLayer = LayerMask.NameToLayer("Environment");
		transform.localScale = Vector3.one;
		mesh = meshFilter.mesh;
		vertices = mesh.vertices;
	}

	protected override void OnValidate()
	{
		base.OnValidate();

		//0.81: �¿���, 1.03: ����
		float extentY = meshFilter.sharedMesh.bounds.extents.y;
		scaleYValue = Mathf.Clamp(0.3f - (extentY - 0.8f), 0.2f, 0.3f);
		transform.localScale = new Vector3(1f, scaleYValue, 1f);

		if (meshFilter.sharedMesh.isReadable == false)
		{
			print($"{gameObject.name}�� Read/Write �Ӽ��� �Ѽ���");
		}
	}

	protected override void Break(bool immediately = false)
	{
		for(int i = 0; i < vertices.Length; i++)
		{
			vertices[i].y += Random.value * 0.5f;
		}
		transform.localScale = new Vector3(1f, scaleYValue, 1f);
		gameObject.layer = environLayer;
		GetComponent<NavMeshObstacle>().enabled = false;
		mesh.vertices = vertices;
		mesh.RecalculateNormals();

		if(immediately == false)
		{
			ParticleSystem vfx = GameManager.Resource.Instantiate<ParticleSystem>(smokeVfxPath, 
				transform.position + Vector3.up, transform.rotation, true);

			var shapeModule = vfx.shape;
			shapeModule.scale = mesh.bounds.size;
			shapeModule.position = mesh.bounds.center;
		}
	}

	public override void BreakEffect(BreakableObjBehaviour.BreakData breakData) { }
}
