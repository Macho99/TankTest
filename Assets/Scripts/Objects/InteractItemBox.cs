using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractItemBox : InteractObject
{

    public enum ItemBoxState { Close, Open }

    [Networked] public ItemBoxState itemBoxState { get; set; }
    public override void Spawned()
    {

    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
