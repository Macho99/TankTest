using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractType
{
    None,
    TreeCut,
    RockBreak,
    IronDig,
    Size
}
[Serializable]
public class InteractData
{
    public string interactHint;
    public InteractType interactType;


}


public abstract class InteractObject : NetworkBehaviour
{

    [SerializeField] protected InteractData interactData;
    [Networked] protected TickTimer currentProgress { get; set; }
    protected float targetTime;
    public enum InteractState { None, Progress, End }

    public event Action<MaterialItem> onComplete;


    public override void Spawned()
    {
        
        interactData = new InteractData();
        interactData.interactHint = "F";
    }
    public virtual bool Detect(out InteractData interactInfo)
    {
           
        interactInfo = interactData;
        return true;

    }
    public override void FixedUpdateNetwork()
    {
        if (currentProgress.IsRunning)
        {
            if (!currentProgress.Expired(Runner))
                return;
            //else
            //{
            //    interactState = InteractState.End;
            //}
        }
    }

    public virtual bool Interact(out InteractObject interactObject)
    {
        //if (isUsed == true && interactState != InteractState.None)
        //{
        //    interactObject = null;
        //    return false;
        //}
        //isUsed = true;
        //interactObject = this;
        interactObject = null;
        return true;
    }

}


