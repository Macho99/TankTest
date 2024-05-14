using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(BreakableObjBehaviour))]
public class BreakableObjBehaviourEditor : Editor
{
	public override void OnInspectorGUI()
	{
		BreakableObjBehaviour behaviour = target as BreakableObjBehaviour;
		if (behaviour == null)
			return;

		if (GUILayout.Button("Reset Obj"))
		{
			behaviour.ObjReset();
		}
		base.OnInspectorGUI();
	}
}