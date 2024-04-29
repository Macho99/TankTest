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
public struct InteractInfo
{
    public string interactHint;
    public InteractType interactType;
}
public abstract class InteractObject : NetworkBehaviour
{
    [SerializeField] protected InteractInfo info;
    [Networked] protected TickTimer currentProgress { get; set; }
    protected float targetTime;
    public enum InteractState { None, Progress, End }

    [SerializeField] protected MaterialItem materialItemPrefab;

    public event Action<MaterialItem> onComplete;
    [Networked, HideInInspector, OnChangedRender(nameof(OnChangeState))] public InteractState interactState { get; set; } = InteractState.None;
    [Networked, HideInInspector] public NetworkBool isUsed { get; set; }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            isUsed = false;
        }
    }
    public virtual bool Detect(out InteractInfo interactInfo)
    {

        if (interactState != InteractState.None)
        {
            interactInfo = default;
            return false;
        }
        Debug.Log(interactState);
        interactInfo = this.info;
        return true;

    }
    public override void FixedUpdateNetwork()
    {
        if (currentProgress.IsRunning)
        {
            if (!currentProgress.Expired(Runner))
                return;
            else
            {
                interactState = InteractState.End;
            }
        }
    }

    public virtual bool Interact(out InteractObject interactObject)
    {
        if (isUsed == true && interactState != InteractState.None)
        {
            interactObject = null;
            return false;
        }
        isUsed = true;
        interactObject = this;

        return true;
    }

    public virtual void StartInteract()
    {
        interactState = InteractState.Progress;

    }

    public float Progress()
    {
        if (!currentProgress.IsRunning)
            return 0f;

        float ratio = (targetTime - (float)currentProgress.RemainingTime(Runner)) / targetTime;

        return Mathf.Floor(ratio * 100f);
    }
    public InteractState GetState()
    {



        return interactState;
    }
    public virtual void Complete()
    {
        MaterialItem item = Instantiate(materialItemPrefab, Vector3.zero, Quaternion.identity);
        onComplete?.Invoke(item);

    }
    public void Cancel()
    {
        currentProgress = TickTimer.None;
        if (HasStateAuthority)
            isUsed = false;

    }

    protected virtual void OnChangeState()
    {
        switch (interactState)
        {
            case InteractState.None:
                Cancel();

                break;

            case InteractState.Progress:
                if (HasStateAuthority)
                    currentProgress = TickTimer.CreateFromSeconds(Runner, targetTime);
                break;

            case InteractState.End:
                if (HasStateAuthority)
                {
                    currentProgress = TickTimer.None;
                    isUsed = false;
                }
                Complete();


                break;
        }
    }
}
