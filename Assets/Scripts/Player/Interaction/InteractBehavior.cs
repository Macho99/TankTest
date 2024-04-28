using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractBehavior
{
    protected InteractType interactType;
    protected bool isStart;
    protected InteractObject interactObject;
    protected PlayerController owner;
    public Action endInteract;
    public InteractBehavior(PlayerController owner, InteractType interactType)
    {
        this.owner = owner;
        this.interactType = interactType;
    }
    public abstract void InteractStart();

    public abstract void InteractLoop();
    public abstract void InteractEnd();

    public abstract void InteractStop();

}
