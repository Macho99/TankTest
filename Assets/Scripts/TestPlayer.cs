using Cinemachine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

public class TestPlayer : NetworkBehaviour
{
	[SerializeField] Vector3 rendererOffset;
	[SerializeField] float jumpForce = 5f;
	[SerializeField] float gravity = -15f;
	[SerializeField] float lookSpeed = 10f;
	[SerializeField] float moveSpeed = 5f;

	TextMeshProUGUI debugText;
	float yAngle;
	[Networked] float xAngle { get; set; }
	new MeshRenderer renderer;
	//float yVel;
	
	SimpleKCC kcc;

	public bool IsGround { get; private set; }

	//[Networked, HideInInspector] public Vector3 Position { get; private set; }
	//[Networked, HideInInspector] public Quaternion Rotation { get; private set; }
	//[Networked, HideInInspector] public Vector3 Velocity { get; private set; }

	private void Awake()
	{
		kcc = GetComponent<SimpleKCC>();
		renderer = GetComponentInChildren<MeshRenderer>();
		debugText = GetComponentInChildren<TextMeshProUGUI>();
	}

	public override void Spawned()
	{
		kcc.enabled = true;
		if (Object.HasInputAuthority == false)
		{
			GetComponentInChildren<CinemachineVirtualCamera>().gameObject.SetActive(false);
		}
	}

	public override void FixedUpdateNetwork()
	{
		if (GetInput(out TestInputData input) == false) return;

		//IsGround = CheckGround();
		//if(IsGround && yVel < 0f)
		//{
		//	yVel = -2f;
		//}
		//else
		//{
		//	yVel += gravity * Runner.DeltaTime;
		//}

		float jump = 0f;
		//print(kcc.IsGrounded);
		if (input.buttons.IsSet(Buttons.Jump) && kcc.IsGrounded)
		{
			jump = jumpForce;
		}

		//transform.position = Position;
		//transform.rotation = Rotation;
		//Vector3 delta = -transform.position;

		//transform.Translate(new Vector3(input.moveVec.x, 0f, input.moveVec.y) * moveSpeed * Runner.DeltaTime, Space.Self);
		yAngle -= input.lookVec.y * Runner.DeltaTime * lookSpeed;
		xAngle += input.lookVec.x * Runner.DeltaTime * lookSpeed;
		xAngle = Mathf.Repeat(xAngle, 360f);
		yAngle = Mathf.Clamp(yAngle, -80f, 80f);
		kcc.SetLookRotation(Quaternion.Euler(0f, xAngle, 0f));
		//transform.rotation = Quaternion.Euler(0f, xAngle, 0f);
		kcc.Move(moveSpeed * transform.TransformDirection(new Vector3(input.moveVec.x, 0f, input.moveVec.y)), jump);
		//cc.Move(Runner.DeltaTime * moveSpeed * transform.TransformDirection(new Vector3(input.moveVec.x, yVel, input.moveVec.y)));

		//delta += transform.position;
		//Velocity = transform.position - Position;
		//Position += delta;
		//Rotation = transform.rotation;
	}

	float lastTickTime;
	int lastTick;

	public override void Render()
	{
		StringBuilder sb = new();
		//sb.AppendLine($"pos: {Position}");
		//sb.AppendLine($"rot: {Rotation.eulerAngles}");
		sb.AppendLine($"xAngle: {xAngle.ToString("F1")}");

		debugText.text = sb.ToString();

		//////////////////////////////////////////////////////////////

		//Vector3 pos = Position;

		//if (Object.IsProxy)
		//{
		//	transform.position = pos;
		//}

		//if (Runner.Tick != lastTick)
		//{
		//	lastTick = Runner.Tick;
		//	lastTickTime = Time.time;
		//}
		//else
		//{
		//	pos += Velocity * (Time.time - lastTickTime);
		//}
		//renderer.transform.position = pos + rendererOffset;
	}

	//private bool CheckGround()
	//{
	//	float checkDist = 0.1f;
	//	Vector3 pos = transform.position;
	//	pos.y += cc.radius - checkDist;
	//	return Physics.CheckSphere(pos, cc.radius, LayerMask.GetMask("Default"));
	//}

	//private void OnDrawGizmos()
	//{
	//	if(cc == null) return;
	//	Gizmos.color = IsGround ? Color.green : Color.red;
	//	float checkDist = 0.1f;
	//	Vector3 pos = transform.position;
	//	pos.y += cc.radius - checkDist;
	//	Gizmos.DrawWireSphere(pos, cc.radius);
	//}
}