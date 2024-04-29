using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractIron : InteractObject
{
    private void Awake()
    {
        targetTime = 10f;
    }
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
    }
}
