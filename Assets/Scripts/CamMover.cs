using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class CamMover : NetworkBehaviour
{
	[SerializeField] float moveSpeed = 4f;
	[SerializeField] float lookSpeed = 5f;
	[Networked] float yAngle { get; set; }
	[Networked]float xAngle { get; set; }
	
	public override void FixedUpdateNetwork()
	{
		if(GetInput(out NetworkInputData input))
		{
			transform.Translate(new Vector3(input.mouseDelta.x, 0f, input.mouseDelta.y) * moveSpeed * Runner.DeltaTime, Space.Self);
			yAngle -= input.mouseDelta.y * Runner.DeltaTime * lookSpeed;
			xAngle += input.mouseDelta.x * Runner.DeltaTime * lookSpeed;
			yAngle = Mathf.Clamp(yAngle, -80f, 80f);
			transform.rotation = Quaternion.Euler(yAngle, xAngle, 0f);
		}
	}
}
