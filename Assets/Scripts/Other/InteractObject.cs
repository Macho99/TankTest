using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractObject : NetworkBehaviour ,IInteractable,IDetectable
{
    public abstract void Detect(out InteractInfo interactInfo);

    public abstract void Interact();
}
