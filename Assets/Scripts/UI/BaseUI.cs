using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseUI : MonoBehaviour
{
	protected Dictionary<string, RectTransform> transforms;
	protected Dictionary<string, Button> buttons;
	protected Dictionary<string, Toggle> toggles;
	protected Dictionary<string, TextMeshProUGUI> texts;
	protected Dictionary<string, Image> images;
	protected RectTransform rectTransform;

	protected virtual void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		BindChildren();
	}

	protected virtual void BindChildren()
	{
		transforms = new Dictionary<string, RectTransform>();
		buttons = new Dictionary<string, Button>();
		toggles = new Dictionary<string, Toggle>();
		texts = new Dictionary<string, TextMeshProUGUI>();
		images = new Dictionary<string, Image>();

		RectTransform[] children = GetComponentsInChildren<RectTransform>(true);
		foreach (RectTransform child in children)
		{
			string key = child.gameObject.name;

			if (transforms.ContainsKey(key))
				continue;

			transforms.Add(key, child);

			Button button = child.GetComponent<Button>();
			if (button != null)
				buttons.Add(key, button);

			TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
			if (text != null)
				texts.Add(key, text);

			Image image = child.GetComponent<Image>();
			if(image != null)
				images.Add(key, image);

			Toggle toggle = child.GetComponent<Toggle>();
			if(toggle != null)
				toggles.Add(key, toggle);
		}
	}

	public abstract void CloseUI();
}