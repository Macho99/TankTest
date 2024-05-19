using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRPGRocket : NetworkBehaviour
{
    [Networked] public Vector3 targetDir { get; set; }
    [SerializeField] private float speed;
    [SerializeField] ParticleSystem particle;
    [SerializeField] private RPGExplostion explostion;
    [Networked] public TickTimer lifeTimer { get; set; }
    private int damage;
    private float lifeTime;
    private void Awake()
    {
        var main = particle.main;
        main.startSpeed = speed;
        lifeTime = 2f;
    }
    public override void Spawned()
    {
        if (!HasStateAuthority)
            particle.Play();
    }
    public void Init(Vector3 hitPoint, int damage)
    {

        this.targetDir = (hitPoint - transform.position).normalized;
        this.damage = damage;
        Debug.DrawRay(transform.position, targetDir, Color.green, 2f);
        transform.forward = targetDir;
        lifeTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
        particle.Play();
    }


    public override void FixedUpdateNetwork()
    {
        if (lifeTimer.ExpiredOrNotRunning(Runner))
        {
            Debug.Log("소환해제");
            Runner.Despawn(Object);
        }
    }
    private void OnParticleCollision(GameObject other)
    {

        if (other.TryGetComponent(out IHittable hittable))
        {
            if (other.layer != LayerMask.NameToLayer("Player"))
            {
                hittable.ApplyDamage(transform, transform.position, transform.forward, damage);

            }

        }
        if (HasStateAuthority)
        {


            List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>(particle.GetSafeCollisionEventSize());
            int numCollisionEvents = particle.GetCollisionEvents(other, collisionEvents);

            // 각 충돌 지점에 대해 동작을 수행합니다.
            for (int i = 0; i < numCollisionEvents; i++)
            {
                Vector3 collisionPoint = collisionEvents[i].intersection;
                Runner.Spawn(explostion, collisionPoint, Quaternion.identity);
                return;
            }
            //Debug.Log(other.transform.position);
            //Debug.Log(other.name);
        }
    }
    private void OnParticleTrigger()
    {
        Debug.Log("OnParticleTrigger");
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter");
    }
}
