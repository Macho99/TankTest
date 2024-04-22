using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCamController : NetworkBehaviour
{
    private Transform followTarget;

    private float rotateYSpeed;

    private void Awake()
    {
        followTarget = transform.Find("FollowTarget").transform;
        rotateYSpeed = 15f;
    }
    public void RotateX(NetworkInputData input)
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


        followTarget.transform.localRotation = Quaternion.Euler(rotX, 0, 0);



    }

}
