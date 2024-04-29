using NUnit.Framework.Internal;
using RayFire;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static Unity.VisualScripting.Dependencies.Sqlite.SQLite3;

[CanEditMultipleObjects]
[CustomEditor(typeof(TestObstacle))]
public class TestObstacleEditor : Editor
{
	public override void OnInspectorGUI()
	{
		TestObstacle obstacle = target as TestObstacle;
		if (obstacle == null)
			return;

		if (GUILayout.Button("Cache Prefab"))
		{
			CachePrefab(obstacle);
		}
		base.OnInspectorGUI();
	}

	public void CachePrefab(TestObstacle obstacle)
	{
		RayfireShatter shatter = obstacle.gameObject.AddComponent<RayfireShatter>();
		shatter.voronoi.amount = obstacle.fragmentCnt;
		shatter.material.iMat = Resources.Load<Material>("Material/ShatterInerMat");
		shatter.Fragment(RayfireShatter.FragLastMode.New);
		SaveFragments(shatter, TestObstacle.cachePath, obstacle.meshFilter.sharedMesh.name, obstacle.colType);

		GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(shatter.fragmentsLast[0].transform.parent.gameObject,
			$"{TestObstacle.cachePath}/{obstacle.cacheName}.prefab");
		if (savedPrefab != null)
		{
			Debug.Log("Prefab created successfully.");
		}
		else
		{
			Debug.LogError("Failed to create prefab.");
		}
		shatter.DeleteFragmentsAll();
		DestroyImmediate(shatter);
	}

	// Save mesh as assets
	private void SaveFragments(RayfireShatter shatter, string path, string saveName, TestObstacle.ColliderType colType)
	{
		// Collect all meshes to save
		bool hasMesh = false;

		// Collect fragments meshes
		List<GameObject> gameObjects = shatter.fragmentsLast;
		if (gameObjects == null)
			return;

		// Collect meshes
		List<Mesh> meshes = new List<Mesh>();
		List<MeshFilter> meshFilters = new List<MeshFilter>();
		foreach (var frag in gameObjects)
		{
			// Get mf
			MeshFilter mf = frag.GetComponent<MeshFilter>();
			meshFilters.Add(mf);

			// No mf
			if (mf == null)
				meshes.Add(null);

			// No mesh
			if (mf != null && mf.sharedMesh == null)
				meshes.Add(null);

			// New mesh
			Mesh tempMesh = Object.Instantiate(mf.sharedMesh);
			tempMesh.name = mf.sharedMesh.name;

			// Collect
			meshes.Add(tempMesh);

			// List has mesh
			hasMesh = true;
		}

		// List has no meshes to save
		if (hasMesh == false)
			return;

		// Export meshes into asset
		ExportMeshes(meshFilters, meshes, path, saveName, colType);
	}

	// Export meshes into asset
	private void ExportMeshes(List<MeshFilter> meshFilters, List<Mesh> meshes, string savePath, string saveName, TestObstacle.ColliderType colType)
	{
		// Empty mesh
		Mesh emptyMesh = new Mesh();
		emptyMesh.name = saveName;

		string finalPath = $"{savePath}/{saveName}.asset";

		// Create asset
		AssetDatabase.CreateAsset(emptyMesh, finalPath);

		// Save each fragment mesh
		for (int i = 0; i < meshFilters.Count; i++)
		{
			// Skip if no mesh
			if (meshFilters[i] == null)
				continue;

			// Apply to meshfilter to avoid save of already referenced mesh
			meshFilters[i].sharedMesh = meshes[i];

			switch (colType)
			{
				case TestObstacle.ColliderType.Mesh:
					MeshCollider meshCol = meshFilters[i].gameObject.AddComponent<MeshCollider>();
					meshCol.sharedMesh = meshes[i];
					meshCol.convex = true;
					break;
				case TestObstacle.ColliderType.Box:
					BoxCollider boxCol = meshFilters[i].gameObject.AddComponent<BoxCollider>();
					boxCol.size = meshFilters[i].sharedMesh.bounds.extents * 2f;
					break;
			}
			meshFilters[i].gameObject.AddComponent<Rigidbody>().mass = 20f;
			meshFilters[i].transform.localScale = Vector3.one * 0.9f;

			// Add all meshes
			AssetDatabase.AddObjectToAsset(meshFilters[i].sharedMesh, finalPath);
		}

		// Save
		AssetDatabase.SaveAssets();
	}
}
