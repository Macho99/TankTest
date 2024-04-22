using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerStandMove : PlayerMove
{
    private float runSpeed;
    private float walkSpeed;

    public PlayerStandMove(float height, float runSpeed, float walkSpeed) : base(height)
    {
        this.runSpeed = runSpeed;
        this.walkSpeed = walkSpeed;
    }

    public override Vector3 SetMove(Transform transform, NetworkInputData input)
    {
        if (input.inputDirection == Vector2.zero)
            moveSpeed = 0f;
        else if (input.buttons.IsSet(NetworkInputData.ButtonType.Run))
            moveSpeed = runSpeed;
        else
            moveSpeed = walkSpeed;

        Vector2 inputDirection = input.inputDirection;

        Vector3 lookRight = new Vector3(transform.right.x, 0f, transform.right.z);
        Vector3 lookForward = new Vector3(transform.forward.x, 0f, transform.forward.z);


        return lookForward * inputDirection.y + lookRight * inputDirection.x;
    }


}
