using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonArea : FXAutoOff
{
	[SerializeField] float loopStopTime = 7f;

	WretchZombie owner;

	bool loopStoped = false;
	ParticleSystem ps;
	LayerMask hitMask;

	protected override void Awake()
	{
		base.Awake();
		ps = GetComponent<ParticleSystem>();
		hitMask = LayerMask.GetMask("Player");
	}

	public void SetOwner(WretchZombie owner)
	{
		this.owner = owner;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		loopStoped = false;
		var mainModule = ps.main;
		mainModule.loop = true;
	}

	protected override void Update()
	{
		base.Update();
		if(loopStoped == false && offTime - loopStopTime < elapsed)
		{
			var mainModule = ps.main;
			mainModule.loop = false;
			loopStoped = true;
		}
	}

	protected override void OnDisable()
	{
		owner = null;
	}

	private void OnParticleCollision(GameObject other)
	{
		if (owner == null) return;

		if (hitMask.IsLayerInMask(other.layer))
		{
			owner.AddPosionHit(other);
		}
	}
}
