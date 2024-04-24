using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractObject : NetworkBehaviour ,IInteractable
{

    public abstract void Detect(out InteractInfo interactInfo);

    public abstract void Interact();
}
