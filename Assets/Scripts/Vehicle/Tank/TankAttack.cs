using Cinemachine;
using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows;

public class TankAttack : VehicleBehaviour
{
	[SerializeField] Transform turretTrans;
	[SerializeField] Transform barrelTrans;
	[SerializeField] Transform firePoint;
	[SerializeField] float turrentRotSpeed = 40f;
	[SerializeField] float barrelRotSpeed = 20f;
	[SerializeField] AnimationCurve depressionCurve;
	[SerializeField] float elevationAngle = 25f;
	[SerializeField] GameObject fireVFX;
	[SerializeField] GameObject hitVFX;
	[SerializeField] float fireCooltime = 8f;
	[SerializeField] float fireRebound = 10000f;

	bool spawned;
	int visualFireCnt;
	Rigidbody rb;

	[Networked, HideInInspector] public float TurretAngle { get; private set; }
	[Networked, HideInInspector] public float BarrelAngle { get; private set; }
	[Networked, HideInInspector] public int FireCnt { get; private set; }
	[Networked, HideInInspector] public Vector3 HitPosition { get; private set; }
	[Networked, HideInInspector] public Vector3 HitNormal { get; private set; }
	[Networked, HideInInspector] public TickTimer FireCooltimer { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		rb = GetComponentInParent<Rigidbody>();
	}

	public override void Spawned()
	{
		base.Spawned();
		if(Object.HasInputAuthority == false)
		{
			cam.gameObject.SetActive(false);
		}
		visualFireCnt = FireCnt;
		spawned = true;
	}

	public override void FixedUpdateNetwork()
	{
		base.FixedUpdateNetwork();
		if (GetInput(out TestInputData input) == false) return;

		Vector3 camForward = Quaternion.Euler(CamYAngle, CamXAngle, 0f) * Vector3.forward;
		RotateTurret(camForward);
		RotateBarrel(camForward);

		if(input.buttons.IsSet(Buttons.Fire) == true)
		{
			if(FireCooltimer.ExpiredOrNotRunning(Runner) == true)
			{
				Fire();
			}
		}
	}

	public override void Render()
	{
		base.Render();
		turretTrans.localRotation = Quaternion.Lerp(turretTrans.localRotation, 
			Quaternion.Euler(0f, TurretAngle, 0f), Time.deltaTime * 2f);
		barrelTrans.localRotation = Quaternion.Lerp(barrelTrans.localRotation, 
			Quaternion.Euler(BarrelAngle, 0f, 0f), Time.deltaTime * 2f);

		if(FireCnt != visualFireCnt)
		{
			Instantiate(fireVFX, firePoint.position, firePoint.rotation);
			if(HitPosition != Vector3.zero)
			{
				Instantiate(hitVFX, HitPosition, Quaternion.LookRotation(HitNormal));
			}
			visualFireCnt = FireCnt;
		}
	}

	private void OnDrawGizmos()
	{
		if(spawned == true)
		{
			Vector3 camForward = Quaternion.Euler(CamYAngle, CamXAngle, 0f) * Vector3.forward;
			Gizmos.DrawLine(transform.position, transform.position + camForward * 10f);
		}
	}

	private void RotateTurret(Vector3 camForward)
	{
		//transform.up을 법선벡터로 가지는 평면에 cam.forward를 투영
		Vector3 turretCamForward = camForward - Vector3.Dot(boarder.transform.up, camForward) * boarder.transform.up;
		float turretAngle = Vector3.SignedAngle(boarder.transform.forward, turretCamForward, boarder.transform.up);
		if (turretAngle < 0f)
		{
			turretAngle += 360f;
		}
		TurretAngle = Mathf.MoveTowardsAngle(TurretAngle, turretAngle, Runner.DeltaTime * turrentRotSpeed);
	}

	private void RotateBarrel(Vector3 camForward)
	{
		float depressionAngle = depressionCurve.Evaluate(Mathf.PingPong(TurretAngle, 180f) / 180f);
		Vector3 barrelCamForward = camForward - Vector3.Dot(turretTrans.right, camForward) * turretTrans.right;
		float barrelAngle = Vector3.SignedAngle(turretTrans.forward, barrelCamForward, turretTrans.right);
		barrelAngle = Mathf.Clamp(barrelAngle, -elevationAngle, -depressionAngle);
		if (barrelAngle < 0f)
		{
			barrelAngle += 360f;
		}
		BarrelAngle = Mathf.MoveTowardsAngle(BarrelAngle, barrelAngle, Runner.DeltaTime * barrelRotSpeed);
	}

	private void Fire()
	{
		FireCooltimer = TickTimer.CreateFromSeconds(Runner, fireCooltime);
		FireCnt++;

		rb.AddForceAtPosition(-barrelTrans.forward * fireRebound, barrelTrans.position, ForceMode.Impulse);

		if (Physics.Raycast(firePoint.position, barrelTrans.forward, out RaycastHit hit, 
			200f, LayerMask.GetMask("Default", "Player")))
		{
			HitPosition = hit.point;
			HitNormal = hit.normal;
			if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
			{
				hit.collider.GetComponentInParent<Rigidbody>().AddForceAtPosition(barrelTrans.forward, hit.point, ForceMode.Impulse);
			}
		}
		else
		{
			HitPosition = Vector3.zero;
			HitNormal = Vector3.zero;
		}
	}
}