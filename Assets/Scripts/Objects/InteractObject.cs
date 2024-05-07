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
public class DetectData
{
    public string interactHint;
    public InteractType interactType;
    public IDetectable detectable;

}


public abstract class InteractObject : NetworkBehaviour, IDetectable
{

    [SerializeField] protected DetectData DetectData;
    protected float targetTime;
    public enum InteractState { None, Progress, End }

    private List<Material> materials;
    protected PlayerInteract playerInteract;
    protected virtual void Awake()
    {
        materials = new List<Material>();
        MeshRenderer[] meshRenderers = transform.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            Material[] prevMaterails = meshRenderer.materials;
            Material[] newMaterails = new Material[prevMaterails.Length + 1];
            for (int i = 0; i < prevMaterails.Length; i++)
            {
                newMaterails[i] = prevMaterails[i];
            }

            newMaterails[prevMaterails.Length] = new Material(GameManager.Resource.Load<Material>("Matarials/InteractMatarial"));
            materials.Add(newMaterails[prevMaterails.Length]);
            meshRenderer.materials = newMaterails;

        }
    }
    public override void Spawned()
    {

        DetectData = new DetectData();
        DetectData.interactHint = "F";
        DetectData.detectable = this;
    }
    public virtual bool Detect(out DetectData interactInfo)
    {
        interactInfo = DetectData;


        return true;

    }



    public abstract void StartInteraction();

    public virtual bool Interact(PlayerInteract playerInteract, out InteractObject interactObject)
    {

        this.playerInteract = playerInteract;
        interactObject = this;
        return true;
    }
    public void IsDetect(bool isDetect)
    {
        float targetAlpha = isDetect == true ? 1f : 0f;
        foreach (Material material in materials)
        {
            material.SetFloat("_Alpha", targetAlpha);
        }
    }

    void IDetectable.OnEnterDetect(out DetectData interactData)
    {
        interactData = this.DetectData;
        IsDetect(true);

    }

    public virtual void OnExitDetect()
    {
        IsDetect(false);
        playerInteract = null;
    }
}


