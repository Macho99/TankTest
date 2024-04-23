using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HostClientDebugUI : MonoBehaviour
{
    private TextMeshProUGUI playerState;

    private void Awake()
    {
        playerState = GetComponent<TextMeshProUGUI>();
    }
    public void SetPlayerInfo(string text)
    {
        this.playerState.text = text;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
