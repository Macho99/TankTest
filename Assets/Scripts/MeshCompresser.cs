using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MeshFilter))]
public class MeshCompresser : MonoBehaviour
{
	[SerializeField] MeshCollider meshCol;
	[SerializeField] float scaleYValue;
	Mesh mesh;
	MeshFilter meshFilter;
	bool compressed = false;
	Vector3[] vertices;

	private void Awake()
	{
		transform.localScale = Vector3.one;
		meshFilter = GetComponent<MeshFilter>();
		mesh = meshFilter.mesh;
		vertices = mesh.vertices;
	}

	private void OnValidate()
	{
		if(meshCol == null)
		{
			meshCol = GetComponent<MeshCollider>();
		}
		//0.81: ½Â¿ëÂ÷, 1.03: ¹ö½º
		float extentY = meshCol.sharedMesh.bounds.extents.y;
		scaleYValue = Mathf.Clamp(0.3f - (extentY - 0.8f), 0.2f, 0.3f);
		transform.localScale = new Vector3(1f, scaleYValue, 1f);
	}

	public void Compress()
	{
		for(int i = 0; i < vertices.Length; i++)
		{
			vertices[i].y += Random.value * 0.5f;
		}
		transform.localScale = new Vector3(1f, scaleYValue, 1f);

		GetComponent<NavMeshObstacle>().enabled = false;
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
		compressed = true;
	}

	private void OnCollisionStay(Collision collision)
	{
		print("collision");
		if(compressed == false)
		{
			Compress();
		}
	}
}
