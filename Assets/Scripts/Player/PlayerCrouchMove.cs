using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCrouchMove : PlayerMove
{
    private float crouchSpeed;


    public PlayerCrouchMove(float height ,float crouchSpeed) :base(height)
    {
        this.crouchSpeed = crouchSpeed;
    }

    public override Vector3 SetMove(Transform transform, NetworkInputData input)
    {
        if (input.inputDirection == Vector2.zero)
            moveSpeed = 0f;
        else
            moveSpeed = crouchSpeed;

        Vector2 inputDirection = input.inputDirection;

        Vector3 lookRight = new Vector3(transform.right.x, 0f, transform.right.z);
        Vector3 lookForward = new Vector3(transform.forward.x, 0f, transform.forward.z);

        return lookForward * inputDirection.y + lookRight * inputDirection.x;
    }


}
