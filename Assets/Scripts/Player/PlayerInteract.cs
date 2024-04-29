using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerInteract : NetworkBehaviour
{
    private float distance;
    [SerializeField] private Transform raycastTr;
    public Action<InteractObject> action;
    public Action<bool, InteractInfo> onDetect;
    private InteractInfo interactInfo;
    [Networked] public NetworkBool IsDetect { get; set; }

    private InteractBehavior[] interactBehaviors;
    private InteractObject interactObject;
    [SerializeField] private ToolItem[] toolItems;
    [SerializeField] private Transform leftHandPivot;
    [SerializeField] private Transform toolItemPivot;
    [SerializeField] private MultiParentConstraint leftParent;
    [Networked] public Vector3 targetPoint { get; set; }
    public float ObjectDistance { get; set; }

    private SimpleKCC kcc;
    public void ActiveToolItem(InteractType interactType, bool isActive)
    {

        toolItems[(int)interactType-1].ActiveToolItem(isActive);

    }
    public InteractObject InteractObject { get { return interactObject; } set { interactObject = value; } }

    public InteractInfo InteractInfo { get { return interactInfo; } set => interactInfo = value; }

    private void Awake()
    {
        kcc = GetComponent<SimpleKCC>();
        ObjectDistance = 1f;
        distance = 3f;
        interactBehaviors = new InteractBehavior[(int)InteractType.Size];
        PlayerController controller = GetComponent<PlayerController>();
        interactBehaviors[(int)InteractType.TreeCut] = new FarmingInteraction(controller, InteractType.TreeCut);
        interactBehaviors[(int)InteractType.RockBreak] = new FarmingInteraction(controller, InteractType.RockBreak);
        //toolItems = new ToolItem[(int)ToolItemType.Size];
    }
    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            SetupLocalPlayerUI();
        }


    }

    public override void FixedUpdateNetwork()
    {

        RaycastDetect();

    }
    public void RaycastDetect()
    {
        if (!CanInteract())
        {
            IsDetect = false;
            onDetect?.Invoke(IsDetect, default);
            return;
        }

        Ray ray = new Ray();
        ray.origin = raycastTr.position;
        ray.direction = raycastTr.transform.forward;

        if (Physics.Raycast(ray, out RaycastHit hit, distance))
        {
            if (hit.collider.TryGetComponent(out InteractObject detectObject))
            {

                if (detectObject.Detect(out InteractInfo interactInfo))
                {
                    if (!this.interactInfo.Equals(interactInfo))
                    {
                        this.interactInfo = interactInfo;
                        IsDetect = true;
                        onDetect?.Invoke(IsDetect, interactInfo);
                    }
                }
            }
            else
            {

                interactInfo = default;
                IsDetect = false;
                onDetect?.Invoke(IsDetect, default);
            }
        }
        else
        {
            interactInfo = default;
            IsDetect = false;
            onDetect?.Invoke(IsDetect, default);
        }

        Debug.DrawRay(ray.origin, ray.direction * distance, Color.red);
    }

    public bool TryInteract()
    {
        if (interactInfo.Equals(default))
            return false;

        if (!CanInteract())
            return false;

        Ray ray = new Ray();
        ray.origin = raycastTr.position;
        ray.direction = raycastTr.transform.forward;

        if (Physics.Raycast(ray, out RaycastHit hit, distance))
        {
            if (hit.collider.TryGetComponent(out InteractObject detectObject))
            {
                if (detectObject.Interact(out InteractObject interactObject))
                {
                    this.interactObject = interactObject;
                    targetPoint = hit.point;

                    return true;
                }
            }
        }
        targetPoint = Vector3.zero;
        return false;

    }
    private void SetupLocalPlayerUI()
    {
        AimUI aimUI = GameManager.UI.ShowSceneUI<AimUI>("UI/PlayerUI/AimUI");
        onDetect += aimUI.ReadInteractInfo;

    }

    public InteractBehavior GetInteractBehavior()
    {
        if (interactInfo.interactType == InteractType.None)
            return null;


        return interactBehaviors[(int)interactInfo.interactType];
    }
    public bool CanInteract()
    {
        if (interactObject != null)
            return false;

        return true;
    }
    public void StopInteract()
    {
        interactObject = null;
        interactInfo = default;
    }


}
