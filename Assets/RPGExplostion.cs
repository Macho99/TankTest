using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGExplostion : NetworkBehaviour
{
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private float explostionSize;
    [SerializeField] private float explostionlifeTime;
    [SerializeField] private LayerMask layerMask;
    [Networked] private TickTimer lifeTimer { get; set; }
    [SerializeField] private int damage;

    private void Awake()
    {

        var lifeTime = particle.main.startLifetime;
        lifeTime.constantMax = explostionlifeTime;

    }
    public override void Spawned()
    {
        lifeTimer = TickTimer.CreateFromSeconds(Runner, explostionlifeTime);
        Collider[] colliders = Physics.OverlapSphere(transform.position, explostionSize, layerMask);
        if (colliders.Length > 0)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].TryGetComponent(out IHittable hittable))
                {
                    Vector3 forceDir = colliders[i].transform.position - transform.position;
                    hittable.ApplyDamage(transform, transform.position, forceDir, damage);
                }
            }
        }
        particle.Play();
    }
    public override void FixedUpdateNetwork()
    {
        if (lifeTimer.ExpiredOrNotRunning(Runner))
        {
            if (HasStateAuthority)
            {
                Debug.Log("익스플로젼해제");
                Runner.Despawn(Object);
            }
        }
    }

}
