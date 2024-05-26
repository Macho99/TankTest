using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPlayer : NetworkBehaviour
{
    [Networked] public NetworkBool isHost { get; private set; }
    [Networked, OnChangedRender(nameof(ReadEvent))] public NetworkBool IsReady { get; private set; }

    private SessionUserUI userUI;
    public event Action onReady;
    public event Action onDespawn;
    [Networked, Capacity(100)] public string UserName { get; private set; }
    [Networked] public int presetIndex { get; set; }
    [Networked] public int ColorIndex { get; set; }
    [Networked] public int HairIndex { get; set; }
    [Networked] public int BreardIndex { get; set; }

    public override void Spawned()
    {

        if (HasInputAuthority)
        {
            isHost = HasStateAuthority;
            UserName = GameManager.auth.Auth.CurrentUser.DisplayName;
            RPC_NicnameSetup(UserName);
            IsReady = false;
            Debug.LogWarning("½ºÆù");
        }
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_NicnameSetup(string nicname)
    {
        UserName = nicname;
        if (userUI != null)
        {
            userUI.UpdateNicname(nicname, Object.StateAuthority);
        }
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        Debug.LogWarning("despawned");
        if (GameManager.network.GetPlayer(runner.LocalPlayer, out NetworkObject player))
        {
            GameManager.network.RemovePlayer(runner.LocalPlayer);
        }
        onDespawn?.Invoke();
        if (userUI != null)
            Destroy(userUI.gameObject);

    }
    public void Setup(SessionUserUI sessionUserUI, PlayerRef playerRef)
    {
        if (HasInputAuthority)
        {
            UserName = GameManager.auth.Auth.CurrentUser.DisplayName;
            RPC_NicnameSetup(GameManager.auth.Auth.CurrentUser.DisplayName);
        }
        Debug.LogWarning(UserName);
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
        if (userUI != null)
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
