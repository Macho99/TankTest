using Cinemachine;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;

public class BasicCamController : NetworkBehaviour
{
    public enum CameraType { None, Aim, Zoom, Size }
    private Transform followTarget;

    private float rotateYSpeed;

    [SerializeField] private CinemachineVirtualCamera mainCam;
    [SerializeField] private CinemachineVirtualCamera aimCam;

    public Transform FollowTarget { get => followTarget; }
    private void Awake()
    {
        followTarget = transform.Find("FollowTarget").transform;
        rotateYSpeed = 15f;
    }
    public override void Spawned()
    {
        if (Object.InputAuthority != Runner.LocalPlayer)
        {
            transform.GetComponentInChildren<CinemachineVirtualCamera>().enabled = false;
            ChangeCamera(CameraType.None);
        }
    }
    public float RotateX(NetworkInputData input)
    {
        float mouseDeltaY = input.mouseDelta.y * rotateYSpeed * Runner.DeltaTime;


        float camAngleX = followTarget.transform.eulerAngles.x;
        float rotX = camAngleX - mouseDeltaY;

        if (rotX < 180f)
        {
            rotX = Mathf.Clamp(rotX, -1f, 50f);
        }
        else
        {
            rotX = Mathf.Clamp(rotX, 335f, 361f);
        }


        return rotX;
    }
    public void ChangeCamera(CameraType cameraType)
    {
        mainCam.Priority = cameraType == CameraType.None ? (int)CameraType.Size : 0;
        aimCam.Priority = cameraType == CameraType.Aim ? (int)CameraType.Size : 0;
    }

}
