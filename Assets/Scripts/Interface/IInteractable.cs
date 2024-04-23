using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InteractInfo
{
    public string interactHint;
}
public interface IInteractable
{
    public void Interact();
}
