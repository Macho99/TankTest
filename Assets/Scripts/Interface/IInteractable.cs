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
public interface IInteractable
{
    public void Interact();
}
