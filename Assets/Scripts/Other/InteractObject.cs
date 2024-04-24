using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractType
{
    None = 0,
    TreeCut
}
[Serializable]
public struct InteractInfo
{
    public string interactHint;
    public InteractType interactType;
}
public abstract class InteractObject : NetworkBehaviour
{
    [SerializeField] protected InteractInfo info;
    public abstract void Detect(out InteractInfo interactInfo);

    public abstract void Interact(PlayerController player, out InteractObject interactObject);

    public abstract void Stop();
}
