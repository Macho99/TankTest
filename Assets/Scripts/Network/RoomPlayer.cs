using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPlayer : NetworkBehaviour
{
    private string userNicname;
    [Networked] public NetworkBool isHost { get; private set; }
    [Networked, OnChangedRender(nameof(ReadEvent))] public NetworkBool IsReady { get; private set; }
    public string UserName { get => userNicname; }

    private SessionUserUI userUI;
    public event Action onReady;
    public event Action onDespawn;
    [Networked] public int presetIndex { get; set; }
    [Networked] public int ColorIndex { get; set; }
    [Networked] public int HairIndex { get; set; }
    [Networked] public int BreardIndex { get; set; }
    public override void Spawned()
    {
        userNicname = GameManager.auth.User.DisplayName;
        if (HasInputAuthority)
        {
            isHost = HasStateAuthority;
            IsReady = false;

        }

    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        onDespawn?.Invoke();
        if (userUI != null)
            Destroy(userUI.gameObject);

    }
    public void Setup(SessionUserUI sessionUserUI, PlayerRef playerRef)
    {
        this.userUI = sessionUserUI;
        userUI.Setup(this, playerRef, IsReady);
        ReadEvent();
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_Ready()
    {
        if (isHost == false)
        {
            IsReady = !IsReady;
        }

    }

    public void StartGame()
    {

        PlayerPreviewController playerPreview = FindObjectOfType<PlayerPreviewController>();
        HairIndex = playerPreview.GetCurrenIndex(AppearanceType.Hair);
        BreardIndex = playerPreview.GetCurrenIndex(AppearanceType.Breard);
        presetIndex = playerPreview.GetCurrenIndex(AppearanceType.Preset);
        ColorIndex = playerPreview.GetCurrenIndex(AppearanceType.Color);


        GameManager.network.JoinIngame();
    }
    public void ReadEvent()
    {
        userUI.ActiveReady(IsReady);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_AddClientPreset(AppearanceType appearanceType, int index)
    {
        switch (appearanceType)
        {
            case AppearanceType.Hair:
                HairIndex = index;
                break;
            case AppearanceType.Breard:
                BreardIndex = index;
                break;
            case AppearanceType.Preset:
                presetIndex = index;
                break;
            case AppearanceType.Color:
                ColorIndex = index;
                break;
        }


    }


}
