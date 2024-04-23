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
    public Action<InteractInfo> onDetect;
    private bool isDetect;
    private void Awake()
    {
        distance = 3f;
        isDetect = false;
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
            if (hit.collider.TryGetComponent(out IDetectable detectObject))
            {
                detectObject.Detect(out InteractInfo interactInfo);
                onDetect?.Invoke(interactInfo);
                isDetect = true;
            }
        }
        else
        {
            onDetect?.Invoke(default);
            isDetect = false;

        }

        Debug.DrawRay(ray.origin, ray.direction * distance, Color.red);
    }
}
