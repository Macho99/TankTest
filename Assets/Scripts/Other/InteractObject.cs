using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractType
{
    None,
    TreeCut,
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

    protected InteractState state;
    public event Action<MaterialItem> onComplete;
    [Networked] public NetworkBool isUsed { get; set; }
    public abstract void Detect(out InteractInfo interactInfo);

    public abstract bool Interact(PlayerController player, out InteractObject interactObject);
    public abstract void Stop();
    public virtual void StartInteract()
    {
        state = InteractState.Progress;
        currentProgress = TickTimer.CreateFromSeconds(Runner, targetTime);
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



        return state;
    }
    public virtual void Complete()
    {
        MaterialItem item = Instantiate(materialItemPrefab, Vector3.zero, Quaternion.identity);
        onComplete?.Invoke(item);

    }
    public abstract void Result();
}
