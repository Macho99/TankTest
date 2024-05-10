using Cinemachine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TestPlayer : NetworkBehaviour, IHittable
{
	[SerializeField] Canvas playerCanvas;
	[SerializeField] Image aimImg;
	[SerializeField] Vector3 rendererOffset;
	[SerializeField] float jumpForce = 5f;
	[SerializeField] float gravity = -15f;
	[SerializeField] float lookSpeed = 600f;
	[SerializeField] float moveSpeed = 5f;
	[SerializeField] int damage = 40;
	[SerializeField] float knockbackPower = 30f;

	Collider col;
	Transform camRoot;
	TextMeshProUGUI debugText;
	SimpleKCC kcc;

	[Networked] public NetworkButtons PrevButton { get; private set; }
	[Networked] public float YAngle { get; private set; }
	[Networked] public float XAngle { get; private set; }
	new MeshRenderer renderer;
	//float yVel;

	public bool IsGround { get; private set; }

	public long HitID => (Object.Id.Raw << 32);

	//[Networked, HideInInspector] public Vector3 Position { get; private set; }
	//[Networked, HideInInspector] public Quaternion Rotation { get; private set; }
	//[Networked, HideInInspector] public Vector3 Velocity { get; private set; }

	private void Awake()
	{
		kcc = GetComponent<SimpleKCC>();
		renderer = GetComponentInChildren<MeshRenderer>();
		debugText = GetComponentInChildren<TextMeshProUGUI>();
		camRoot = GetComponentInChildren<CinemachineVirtualCamera>().transform.parent;
	}

	public override void Spawned()
	{
		col = GetComponentInChildren<Collider>();
		kcc.enabled = true;
		if (HasInputAuthority == false)	
		{
			camRoot.gameObject.SetActive(false);
			playerCanvas.gameObject.SetActive(false);
		}
		else
		{
			XAngle = Vector3.Angle(Vector3.forward, transform.forward);
		}
	}

	public override void FixedUpdateNetwork()
	{
		if (GetInput(out TestInputData input) == false) return;

		Move(input);
		CheckFire(input);
	}

	public override void Render()
	{
		//StringBuilder sb = new();
		////sb.AppendLine($"pos: {Position}");
		////sb.AppendLine($"rot: {Rotation.eulerAngles}");
		//sb.AppendLine($"xAngle: {xAngle.ToString("F1")}");

		//debugText.text = sb.ToString();
	}

	private void Move(TestInputData input)
	{
		float jump = 0f;
		//print(kcc.IsGrounded);
		if (input.buttons.IsSet(Buttons.Jump) && kcc.IsGrounded)
		{
			jump = jumpForce;
		}

		YAngle -= input.lookVec.y * Runner.DeltaTime * lookSpeed;
		XAngle += input.lookVec.x * Runner.DeltaTime * lookSpeed;
		XAngle = Mathf.Repeat(XAngle, 360f);
		YAngle = Mathf.Clamp(YAngle, -80f, 80f);
		camRoot.localRotation = Quaternion.Euler(YAngle, 0f, 0f);
		kcc.SetLookRotation(Quaternion.Euler(0f, XAngle, 0f));
		kcc.Move(moveSpeed * transform.TransformDirection(new Vector3(input.moveVec.x, 0f, input.moveVec.y)), jump);
	}

	private void CheckFire(TestInputData input)
	{
		NetworkButtons pressed = input.buttons.GetPressed(PrevButton);
		NetworkButtons released = input.buttons.GetReleased(PrevButton);

		PrevButton = input.buttons;

		if (pressed.IsSet(Buttons.Fire))
		{
			aimImg.color = Color.red;
			Fire();
		}
		if (pressed.IsSet(Buttons.Interact))
		{
			CheckTank();
		}
		if (released.IsSet(Buttons.Fire))
		{
			aimImg.color = Color.white;
		}
	}

	private void CheckTank()
	{
		if(Physics.Raycast(camRoot.position, camRoot.forward, 
			out RaycastHit hitInfo, 10f, LayerMask.GetMask("Vehicle")) == true)
		{
			VehicleBoarder tank = hitInfo.collider.GetComponentInParent<VehicleBoarder>();
			if(tank == null)
			{
				print(hitInfo.collider.name);
				return;
			}
			tank.GetOn(this);
		}
	}

	private void Fire()
	{
		if (Physics.Raycast(camRoot.position, camRoot.forward, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Monster"), QueryTriggerInteraction.Collide))
		{
			hitInfo.collider.GetComponent<IHittable>().ApplyDamage(transform,
				hitInfo.point, camRoot.forward * knockbackPower, damage);
		}
	}

	public void CollisionEnable(bool value)
	{
		col.enabled = value;
	}

	public void KCCActive(bool value)
	{
		kcc.SetActive(value);
	}

	public void Teleport(Vector3 position)
	{
		kcc.SetPosition(position);
	}

	public void ApplyDamage(Transform source, Vector3 point, Vector3 force, int damage)
	{
		print($"{source.name}로부터 {damage} 데미지");
	}
}