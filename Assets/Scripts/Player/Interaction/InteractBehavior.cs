using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractBehavior
{

    public abstract bool InteractStart();

    public abstract void InteractLoop();
    public abstract bool InteractEnd();

    public abstract void InteractStop();

}
