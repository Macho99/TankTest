using Cinemachine;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;

using Color = UnityEngine.Color;
public class BasicCamController : NetworkBehaviour
{
    public enum CameraType { None, Aim, Zoom, Size }
    private Transform followTarget;

    private float rotateYSpeed;

    [SerializeField] private CinemachineVirtualCamera mainCam;
    [SerializeField] private CinemachineVirtualCamera aimCam;
    //[SerializeField] private Transform raycasterTr;
    [SerializeField] private Transform rayCastCam;



    public Transform RayCasterTr { get => rayCastCam; }
    public Transform FollowTarget { get => followTarget; }
    private void Awake()
    {
        followTarget = transform.Find("FollowTarget").transform;
        rotateYSpeed = 15f;


    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RayCast(Vector3 position, Quaternion rotation)
    {
        rayCastCam.transform.position = position;
        rayCastCam.transform.rotation = rotation;
    }
    public override void FixedUpdateNetwork()
    {
        if (HasInputAuthority)
        {
            rayCastCam.transform.position = Camera.main.transform.position;
            rayCastCam.transform.rotation = Camera.main.transform.rotation;
            RPC_RayCast(rayCastCam.transform.position, rayCastCam.transform.rotation);
        }
        Debug.DrawRay(rayCastCam.transform.position, rayCastCam.transform.forward * 100, Color.blue);
    }

    public override void Spawned()
    {
        if (!HasInputAuthority)
        {

            CinemachineVirtualCamera[] cameras = transform.GetComponentsInChildren<CinemachineVirtualCamera>();

            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].enabled = false;
            }

        }
        ChangeCamera(CameraType.None);
    }
    public float RotateX(NetworkInputData input, float rotX)
    {
        float mouseDeltaY = input.mouseDelta.y * rotateYSpeed * Runner.DeltaTime;


        //float camAngleX = followTarget.transform.eulerAngles.x;
        float newRotX = rotX - mouseDeltaY;

        newRotX = Mathf.Clamp(newRotX, -70, 70);
        //if (newRotX < 180f)
        //{
        //    newRotX = Mathf.Clamp(newRotX, -1f, 70f);
        //}
        //else
        //{
        //    newRotX = Mathf.Clamp(newRotX, 320f, 361f);
        //}


        return newRotX;
    }


    public void ChangeCamera(CameraType cameraType)
    {

        mainCam.Priority = cameraType == CameraType.None ? (int)CameraType.Size : 0;
        aimCam.Priority = cameraType == CameraType.Aim ? (int)CameraType.Size : 0;
    }
    public void ResetCamera()
    {
        mainCam.Priority = (int)CameraType.Size;
        aimCam.Priority = 0;
    }

  
}
