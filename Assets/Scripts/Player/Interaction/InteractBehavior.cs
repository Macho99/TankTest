using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractBehavior
{

    public InteractBehavior(InteractType interactType)
    {

    }
    protected InteractType interactType;
    public abstract bool InteractStart();

    public abstract void InteractLoop();
    public abstract bool InteractEnd();

    public abstract void InteractStop();

}
