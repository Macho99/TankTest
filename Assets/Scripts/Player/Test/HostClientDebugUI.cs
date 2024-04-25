using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HostClientDebugUI : SceneUI
{
    [SerializeField]private TextMeshProUGUI playerState;
    [SerializeField] private RectTransform contentTr;
    [SerializeField] private TextMeshProUGUI debugTMPPrefab;
    protected override void Awake()
    {
        base.Awake();
    }
    public void SetPlayerInfo(string text)
    {
        this.playerState.text = text;
    }
    public void AddDebugText(string text)
    {
        TextMeshProUGUI debugText = Instantiate(debugTMPPrefab, contentTr);
        debugText.text = text;
    }
    public void ClearAllText()
    {
        foreach (Transform child in contentTr.transform)
        {
            Destroy(child.gameObject);
        }
    }

}
