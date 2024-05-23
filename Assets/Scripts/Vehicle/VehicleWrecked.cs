using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class VehicleWrecked : NetworkBehaviour
{
	public enum WreckPhase { Explosion, FireAndSmoke, Smoke, CalmDown }

	const string fireVfxPath = "FX/VFX/vfx_Fire01_v1";
	const string smokeVfxPath = "FX/VFX/vfxgraph_StylizedSmoke_v1.5";
	const string explosionVfxPath = "FX/VFX/BigExplosionv2";

	[SerializeField] AudioClip explosionClip;
	[SerializeField] float velocityMul = 2f;
	[SerializeField] Vector3 smokeRadius = Vector3.one;
	[SerializeField] Transform wheelRendererTrans;
	[SerializeField] Transform wheelColTrans;
	[SerializeField] Transform fireTrans;

	[SerializeField] Transform[] wheelTranses;
	[SerializeField] WheelCollider[] wheelColliders;

	AudioSource audioSource;
	Rigidbody rb;
	Queue<GameObject> fireQueue = new();
	Queue<GameObject> smokeQueue = new();

	TickTimer phaseTimer;
	[Networked, OnChangedRender(nameof(PhaseChange))] public WreckPhase Phase { get; private set; } = WreckPhase.Explosion;
	[Networked] public float Rpm { get; private set; }

	protected virtual void Awake()
	{
		rb = GetComponent<Rigidbody>();
		audioSource = GetComponent<AudioSource>();
	}

	public override void Spawned()
	{
		if (HasStateAuthority)
		{
			phaseTimer = TickTimer.CreateFromSeconds(Runner, 2f);
		}
		PhaseChange();

		if(explosionClip != null)
			audioSource.PlayOneShot(explosionClip);
	}

	public override void FixedUpdateNetwork()
	{
		SetRpm();
		CheckPhase();
	}

	protected virtual void ExplosionForce()
	{
		Vector3 velocity = Vector3.up * 10f + Random.insideUnitSphere * 3f;
		velocity *= velocityMul;
		rb.AddForceAtPosition(velocity, fireTrans.position + Random.insideUnitSphere * 2f, ForceMode.VelocityChange);
	}

	protected virtual void PlayExplosionVfx()
	{
		GameManager.Resource.Instantiate<FXAutoOff>(explosionVfxPath, fireTrans.position, fireTrans.rotation, true);
	}

	protected void PhaseChange()
	{
		if(Phase == WreckPhase.Explosion)
		{
			if (HasStateAuthority)
			{
				ExplosionForce();
			}
			PlayExplosionVfx();
			MakeFire();
			MakeSmoke();
		}
		else if(Phase == WreckPhase.FireAndSmoke)
		{
			if(fireQueue.Count == 0)
			{
				MakeFire();
			}
			if(smokeQueue.Count == 0)
			{
				MakeSmoke();
			}
		}
		else if(Phase == WreckPhase.Smoke)
		{
			RemoveFire();
			if (smokeQueue.Count == 0)
			{
				MakeSmoke();
			}
		}
		else if(Phase == WreckPhase.CalmDown)
		{
			RemoveFire();
			RemoveSmoke();
		}
	}

	private void MakeFire()
	{
		int fireCnt = Random.Range(1, 3);
		for (int i = 0; i < fireCnt; i++)
		{
			fireQueue.Enqueue(GameManager.Resource.Instantiate<GameObject>(fireVfxPath, 
				fireTrans.position + Random.insideUnitSphere * 0.5f, fireTrans.rotation, fireTrans, true));
		}
	}

	private void MakeSmoke()
	{
		int smokeCnt = Random.Range(3, 5);
		for (int i = 0; i < smokeCnt; i++)
		{
			smokeQueue.Enqueue(GameManager.Resource.Instantiate<GameObject>(smokeVfxPath, (transform.position + Vector3.up) + 
				transform.rotation * new Vector3(Random.value * smokeRadius.x, Random.value * smokeRadius.y, Random.value * smokeRadius.z), 
				transform.rotation, transform, true));
		}
	}

	private void RemoveFire()
	{
		while (fireQueue.Count > 0)
		{
			fireQueue.Dequeue().AddComponent<VisualEffectOff>().Init(Random.Range(0f, 5f), 5f);
			//GameManager.Resource.Destroy(fireQueue.Dequeue());
		}
	}

	private void RemoveSmoke()
	{
		while (smokeQueue.Count > 0)
		{
			smokeQueue.Dequeue().AddComponent<VisualEffectOff>().Init(Random.Range(0f, 5f), 5f);
			//GameManager.Resource.Destroy(smokeQueue.Dequeue());
		}

		audioSource.Stop();
	}

	private void CheckPhase()
	{
		if (phaseTimer.ExpiredOrNotRunning(Runner) == false) return;

		switch (Phase) 
		{
			case WreckPhase.Explosion:
				phaseTimer = TickTimer.CreateFromSeconds(Runner, 10f);
				Phase++;
				break;
			case WreckPhase.FireAndSmoke:
				phaseTimer = TickTimer.CreateFromSeconds(Runner, 10f);
				Phase++;
				break;
			case WreckPhase.Smoke:
				Phase++;
				break;
			case WreckPhase.CalmDown:
			default:
				break;
		}
	}

	private void SetRpm()
	{
		float rpm = 0f;
		for (int i = 0; i < wheelColliders.Length; i++)
		{
			rpm += wheelColliders[i].rpm;
		}
		rpm /= wheelColliders.Length;
		Rpm = rpm;
	}

	public override void Render()
	{
		float angle = Rpm * 6f * Time.deltaTime;
		for (int i = 0; i < wheelTranses.Length; i++)
		{
			wheelColliders[i].GetWorldPose(out Vector3 pos, out Quaternion rot);
			wheelTranses[i].position = pos;
			wheelTranses[i].Rotate(Vector3.right, angle);
		}
	}

	private void OnValidate()
	{
		if(wheelRendererTrans != null)
		{
			if (wheelTranses == null || wheelTranses.Length == 0)
			{
				wheelTranses = new Transform[wheelRendererTrans.childCount];
				for (int i = 0; i < wheelRendererTrans.childCount; i++)
				{
					wheelTranses[i] = wheelRendererTrans.GetChild(i);
				}
			}
		}
		if(wheelColTrans != null)
		{
			if (wheelColliders == null || wheelColliders.Length == 0)
			{
				wheelColliders = wheelColTrans.GetComponentsInChildren<WheelCollider>();
			}
		}
	}
}