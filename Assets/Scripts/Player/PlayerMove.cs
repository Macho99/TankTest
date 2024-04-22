using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerMove
{

    protected float moveSpeed;
    protected float height;

    public PlayerMove(float height)
    {
        this.height = height;
    }

    public float Height { get { return height; } }
    public float MoveSpeed { get => moveSpeed; }
    public abstract Vector3 SetMove(Transform transform, NetworkInputData input);

    public virtual void StopMove()
    {
        moveSpeed = 0f;
    }
}
