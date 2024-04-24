using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : NetworkBehaviour
{
    private float distance;
    [SerializeField] private Transform raycastTr;
    public Action<InteractObject> action;
    public Action<bool, InteractInfo> onDetect;
    private InteractInfo interactInfo;
    private bool isDetect;

    private InteractBehavior[] interactBehavior;

    public InteractInfo InteractInfo { get { return interactInfo; } }
    public bool IsDetect { get { return isDetect; } }
    private void Awake()
    {
        distance = 3f;
    }
    public override void Spawned()
    {
        if (Object.InputAuthority == Runner.LocalPlayer)
            SetupLocalPlayerUI();

    }
    public override void FixedUpdateNetwork()
    {
        RaycastDetect();
    }


    public void RaycastDetect()
    {
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
                    isDetect = true;
                    onDetect?.Invoke(isDetect, interactInfo);
                }

            }
            else
            {
                interactInfo = default;
                isDetect = false;
                onDetect?.Invoke(isDetect, default);
            }
        }
        else
        {
            interactInfo = default;
            isDetect = false;
            onDetect?.Invoke(isDetect, default);
            Debug.Log("¾ø");
        }

        Debug.DrawRay(ray.origin, ray.direction * distance, Color.red);
    }

    public bool TryInteract()
    {
        if (interactInfo.Equals(default))
            return false;




        return true;

    }
    private void SetupLocalPlayerUI()
    {
        AimUI aimUI = GameManager.UI.ShowSceneUI<AimUI>("UI/PlayerUI/AimUI");
        onDetect += aimUI.ReadInteractInfo;

    }
}
