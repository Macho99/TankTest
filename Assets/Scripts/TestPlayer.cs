using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : NetworkBehaviour
{
	[SerializeField] float lookSpeed = 10f;
	[SerializeField] float moveSpeed = 5f;
	float yAngle;
	float xAngle;
	new MeshRenderer renderer;

	CharacterController cc;

	[Networked] public Vector3 Position { get; private set; }
	[Networked] public Quaternion Rotation { get; private set; }
	[Networked] public Vector3 Velocity { get; private set; }

	private void Awake()
	{
		cc = GetComponent<CharacterController>();
		renderer = GetComponentInChildren<MeshRenderer>();
	}

	public override void FixedUpdateNetwork()
	{
		if (GetInput(out TestInputData input))
		{
			//transform.Translate(new Vector3(input.moveVec.x, 0f, input.moveVec.y) * moveSpeed * Runner.DeltaTime, Space.Self);
			cc.Move(Runner.DeltaTime * moveSpeed * transform.TransformDirection(new Vector3(input.moveVec.x, 0f, input.moveVec.y)));
			yAngle -= input.lookVec.y * Runner.DeltaTime * lookSpeed;
			xAngle += input.lookVec.x * Runner.DeltaTime * lookSpeed;
			yAngle = Mathf.Clamp(yAngle, -80f, 80f);
			transform.rotation = Quaternion.Euler(0f, xAngle, 0f);

			Velocity = transform.position - Position;
			Position = transform.position;
			Rotation = transform.rotation;
		}
	}

	float lastTickTime;
	int lastTick;
	int tickCnt;
	public override void Render()
	{
		renderer.transform.rotation = Rotation;
		renderer.transform.position = Position + Velocity * (Time.time - lastTickTime);

		if(Runner.Tick != lastTick)
		{
			lastTick = Runner.Tick;
			lastTickTime = Time.time;
			tickCnt = 0;
		}
		else
		{
			tickCnt++;
		}
		print(tickCnt);
	}
}