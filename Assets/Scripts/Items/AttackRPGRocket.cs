using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRPGRocket : NetworkBehaviour
{
    [Networked] public NetworkBool isFire { get; set; }
    [Networked] public Vector3 targetPoint { get; set; }
    private float speed;
    private void Awake()
    {
        speed = 10f;
    }
    public override void Spawned()
    {

    }

    public void OnFire(Vector3 targetPoint)
    {
        isFire = true;
        this.targetPoint = targetPoint;
    }

    public override void FixedUpdateNetwork()
    {
        if (isFire)
        {
            Vector3 direction = (targetPoint - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.position += direction * speed * Runner.DeltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isFire)
        {
            Runner.Despawn(Object);
        }
    }
}
