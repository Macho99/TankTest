using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractTree : InteractObject
{
    private void Awake()
    {
        targetTime = 5f;
    }
  
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();    
    }


}
