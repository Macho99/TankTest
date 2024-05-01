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
    public Action<bool, InteractData> onDetect;
    private InteractData interactData;
    [Networked] public NetworkBool IsDetect { get; set; }

    private InteractBehavior[] interactBehaviors;
    private InteractObject interactObject;
    [SerializeField] private Transform leftHandPivot;
    [SerializeField] private Transform toolItemPivot;
    [SerializeField] private MultiParentConstraint leftParent;
    [Networked] public Vector3 targetPoint { get; set; }
    public float ObjectDistance { get; set; }

    private SimpleKCC kcc;

    public InteractObject InteractObject { get { return interactObject; } set { interactObject = value; } }

    public InteractData InteractData { get { return interactData; } set => interactData = value; }

    private void Awake()
    {
        kcc = GetComponent<SimpleKCC>();
        ObjectDistance = 1f;
        distance = 3f;
        interactBehaviors = new InteractBehavior[(int)InteractType.Size];
        PlayerController controller = GetComponent<PlayerController>();
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

        // RaycastDetect();

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

                if (detectObject.Detect(out InteractData interactData))
                {

                    this.interactData = interactData;
                    IsDetect = true;
                    onDetect?.Invoke(IsDetect, this.interactData);

                }
            }
            else
            {

                interactData = default;
                IsDetect = false;
                onDetect?.Invoke(IsDetect, default);
            }
        }
        else
        {
            interactData = default;
            IsDetect = false;
            onDetect?.Invoke(IsDetect, default);
        }

        Debug.DrawRay(ray.origin, ray.direction * distance, Color.red);
    }

    public bool TryInteract()
    {
        if (interactData.Equals(default))
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
        //onDetect += aimUI.ReadInteractInfo;

    }

    public InteractBehavior GetInteractBehavior()
    {
        if (interactData.interactType == InteractType.None)
            return null;


        return interactBehaviors[(int)interactData.interactType];
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
        interactData = default;
    }


}
