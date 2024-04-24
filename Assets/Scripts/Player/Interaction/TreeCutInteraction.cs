using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeCutInteraction : InteractBehavior
{

    public TreeCutInteraction() : base(InteractType.TreeCut)
    {

    }


    public override bool InteractEnd()
    {
        return true;
    }

    public override void InteractLoop()
    {

    }

    public override bool InteractStart()
    {
        return true;
    }

    public override void InteractStop()
    {

    }
}
