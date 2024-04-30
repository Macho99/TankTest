using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CompressObstacle : BreakableObstacle
{
	[SerializeField] float scaleYValue;
	Mesh mesh;
	Vector3[] vertices;

	protected override void Awake()
	{
		base.Awake();
		transform.localScale = Vector3.one;
		mesh = meshFilter.mesh;
		vertices = mesh.vertices;
	}

	protected override void OnValidate()
	{
		base.OnValidate();

		//0.81: ½Â¿ëÂ÷, 1.03: ¹ö½º
		float extentY = meshFilter.sharedMesh.bounds.extents.y;
		scaleYValue = Mathf.Clamp(0.3f - (extentY - 0.8f), 0.2f, 0.3f);
		transform.localScale = new Vector3(1f, scaleYValue, 1f);
	}

	protected override void Break(bool immediately = false)
	{
		for(int i = 0; i < vertices.Length; i++)
		{
			vertices[i].y += Random.value * 0.5f;
		}
		transform.localScale = new Vector3(1f, scaleYValue, 1f);

		GetComponent<NavMeshObstacle>().enabled = false;
		mesh.vertices = vertices;
		mesh.RecalculateNormals();

		if(immediately == false)
		{

		}
	}
}
