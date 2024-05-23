using UnityEngine;
using UnityEditor;
using TMPro;

public class SetFontForTextMeshPro : EditorWindow
{
	private TMP_FontAsset fontAsset;

	[MenuItem("Tools/Set Font For TextMeshPro")]
	public static void ShowWindow()
	{
		GetWindow<SetFontForTextMeshPro>("Set Font For TextMeshPro");
	}

	void OnGUI()
	{
		GUILayout.Label("Set Font For TextMeshPro", EditorStyles.boldLabel);

		fontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField("Font Asset", fontAsset, typeof(TMP_FontAsset), false);

		if (GUILayout.Button("Set Font"))
		{
			SetFont();
		}
	}

	void SetFont()
	{
		if (fontAsset == null)
		{
			Debug.LogError("No font asset selected!");
			return;
		}

		// Find all TextMeshProUGUI objects in the scene
		TextMeshProUGUI[] textObjects = FindObjectsOfType<TextMeshProUGUI>(true);

		foreach (TextMeshProUGUI textObject in textObjects)
		{
			Undo.RecordObject(textObject, "Set Font");
			textObject.font = fontAsset;
			EditorUtility.SetDirty(textObject);
		}

		// Find all TextMeshPro objects in the scene (for 3D text)
		TextMeshPro[] textMeshObjects = FindObjectsOfType<TextMeshPro>(true);

		foreach (TextMeshPro textMeshObject in textMeshObjects)
		{
			Undo.RecordObject(textMeshObject, "Set Font");
			textMeshObject.font = fontAsset;
			EditorUtility.SetDirty(textMeshObject);
		}

		Debug.Log("Font set for all TextMeshPro objects in the scene.");
	}
}