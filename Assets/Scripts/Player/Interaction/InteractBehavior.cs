using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractBehavior
{
    protected Animator animator;
    protected InteractType interactType;
    public InteractBehavior(Animator animator,InteractType interactType)
    {
        this.animator = animator;
        this.interactType = interactType;   
    }
    public abstract bool InteractStart();

    public abstract void InteractLoop();
    public abstract bool InteractEnd();

    public abstract void InteractStop();

}
