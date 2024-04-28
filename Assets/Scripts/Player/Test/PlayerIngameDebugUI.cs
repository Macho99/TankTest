using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerIngameDebugUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI playerTypeTMP;
    [SerializeField] private DebugLogTextItem debugTextPrefab;
    [SerializeField] private RectTransform contentTr;
    private Dictionary<string, DebugLogTextItem> debugTexts;
    private void Awake()
    {
        debugTexts = new Dictionary<string, DebugLogTextItem>();
    }

    private void LateUpdate()
    {
        Quaternion rotate = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);
        transform.rotation = rotate;
    }
    public void SetPlayerType(string playerType)
    {
        playerTypeTMP.text = playerType;
    }
    public void AddDebugText(string title, string content)
    {
        if (debugTexts.ContainsKey(title))
        {
            debugTexts[title].SetText(title, content);
            return;

        }

        DebugLogTextItem debugItem = Instantiate(debugTextPrefab, contentTr);
        debugItem.SetText(title, content);

        debugTexts.Add(title, debugItem);

    }
    public void AllClearDubugText()
    {
        if (debugTexts.Count <= 0)
            return;

        foreach (DebugLogTextItem item in debugTexts.Values)
        {
            Destroy(item.gameObject);

        }
        debugTexts.Clear();
    }

}
