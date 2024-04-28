using Fusion;
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


    public void ActiveToolItem(ToolItemType toolType, bool isActive)
    {

        toolItems[(int)toolType].ActiveToolItem(isActive);

    }
    public InteractObject InteractObject { get { return interactObject; } set { interactObject = value; } }

    public InteractInfo InteractInfo { get { return interactInfo; } set => interactInfo = value; }

    private void Awake()
    {
        distance = 3f;
        interactBehaviors = new InteractBehavior[(int)InteractType.Size];
        PlayerController controller = GetComponent<PlayerController>();
        interactBehaviors[(int)InteractType.TreeCut] = new TreeCutInteraction(controller);
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

                detectObject.Detect(out InteractInfo interactInfo);
                if (!this.interactInfo.Equals(interactInfo))
                {
                    this.interactInfo = interactInfo;
                    IsDetect = true;
                    onDetect?.Invoke(IsDetect, interactInfo);
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
                if (detectObject.Interact(this.GetComponent<PlayerController>(), out InteractObject interactObject))
                    this.interactObject = interactObject;
                else
                    return false;

            }
        }

        return true;

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
