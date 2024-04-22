using Cinemachine;
using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.Windows;

public class TankAttack : NetworkBehaviour
{
	[SerializeField] Transform cam;
	[SerializeField] float lookSpeed = 20f;
	[SerializeField] Transform turretTrans;
	[SerializeField] Transform barrelTrans;
	[SerializeField] Transform firePoint;
	[SerializeField] float turrentRotSpeed = 40f;
	[SerializeField] float barrelRotSpeed = 20f;
	[SerializeField] float depressionAngle = -10f;
	[SerializeField] float elevationAngle = 25f;
	[SerializeField] GameObject fireVFX;
	[SerializeField] GameObject hitVFX;
	[SerializeField] float fireCooltime = 8f;
	[SerializeField] float fireRebound = 10000f;

	int visualFireCnt;
	Rigidbody rb;

	[Networked, HideInInspector] public float CamYAngle { get; private set; }
	[Networked, HideInInspector] public float CamXAngle { get; private set; }
	[Networked, HideInInspector] public float TurretAngle { get; private set; }
	[Networked, HideInInspector] public float BarrelAngle { get; private set; }
	[Networked, HideInInspector] public int FireCnt { get; private set; }
	[Networked, HideInInspector] public Vector3 HitPosition { get; private set; }
	[Networked, HideInInspector] public Vector3 HitNormal { get; private set; }
	[Networked, HideInInspector] public TickTimer FireCooltimer { get; private set; }

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	public override void Spawned()
	{
		if(Object.HasInputAuthority == false)
		{
			cam.gameObject.SetActive(false);
		}
		visualFireCnt = FireCnt;
	}

	public override void FixedUpdateNetwork()
	{
		if (GetInput(out TestInputData input) == false) return;

		RotateCam(input);
		RotateTurret();
		RotateBarrel();

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
		turretTrans.localRotation = Quaternion.Euler(0f, TurretAngle, 0f);
		barrelTrans.localRotation = Quaternion.Euler(BarrelAngle, 0f, 0f);

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
		//Gizmos.color = Color.blue;
		//Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5f);
		//Gizmos.color = Color.red;
		//Vector3 barrelCamForward = cam.forward - Vector3.Dot(transform.right, cam.forward) * transform.right;
		//Gizmos.DrawLine(transform.position, transform.position + barrelCamForward * 5f);
	}

	private void RotateCam(TestInputData input)
	{
		CamYAngle -= input.lookVec.y * Runner.DeltaTime * lookSpeed;
		CamXAngle += input.lookVec.x * Runner.DeltaTime * lookSpeed;
		CamXAngle = Mathf.Repeat(CamXAngle, 360f);
		CamYAngle = Mathf.Clamp(CamYAngle, -40f, 40f);
		cam.rotation = Quaternion.Euler(CamYAngle, CamXAngle, 0f);
	}

	private void RotateTurret()
	{
		//transform.up을 법선벡터로 가지는 평면에 cam.forward를 투영
		Vector3 turretCamForward = cam.forward - Vector3.Dot(transform.up, cam.forward) * transform.up;
		float turretAngle = Vector3.SignedAngle(transform.forward, turretCamForward, transform.up);
		if (turretAngle < 0f)
		{
			turretAngle += 360f;
		}
		TurretAngle = Mathf.MoveTowardsAngle(TurretAngle, turretAngle, Runner.DeltaTime * turrentRotSpeed);
	}

	private void RotateBarrel()
	{
		Vector3 barrelCamForward = cam.forward - Vector3.Dot(turretTrans.right, cam.forward) * turretTrans.right;
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