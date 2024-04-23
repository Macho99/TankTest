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

public class TestPlayer : NetworkBehaviour
{
	[SerializeField] Canvas playerCanvas;
	[SerializeField] Image aimImg;
	[SerializeField] Vector3 rendererOffset;
	[SerializeField] float jumpForce = 5f;
	[SerializeField] float gravity = -15f;
	[SerializeField] float lookSpeed = 600f;
	[SerializeField] float moveSpeed = 5f;
	[SerializeField] int damage = 40;

	Transform camRoot;
	TextMeshProUGUI debugText;
	SimpleKCC kcc;

	[Networked] public NetworkButtons PrevButton { get; private set; }
	[Networked] public float YAngle { get; private set; }
	[Networked] public float XAngle { get; private set; }
	new MeshRenderer renderer;
	//float yVel;

	public bool IsGround { get; private set; }

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
		if (released.IsSet(Buttons.Fire))
		{
			aimImg.color = Color.white;
		}
	}

	private void Fire()
	{
		if(Physics.Raycast(camRoot.position, camRoot.forward, out RaycastHit hitInfo, 100f, LayerMask.GetMask("Monster"), QueryTriggerInteraction.Collide))
		{
			hitInfo.collider.GetComponent<IHittable>().ApplyDamage(transform, hitInfo.point, camRoot.forward * 300f, damage);
		}

		//if (Runner.LagCompensation.Raycast(camRoot.position, camRoot.forward,
		//	100f, Runner.LocalPlayer, out var hit, options: HitOptions.IgnoreInputAuthority))
		//{
		//	if (hit.Hitbox == null)
		//	{
		//		return;
		//	}

		//	if (hit.Hitbox is ZombieHitBox zombieHitBox)
		//	{
		//		zombieHitBox.ApplyDamage(transform, transform.forward * 30f, 40);
		//	}
		//}
	}
}