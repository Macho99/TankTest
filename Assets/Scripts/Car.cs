using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Car : MonoBehaviour
{
	[SerializeField] float rotationAngle = 35f;
	[SerializeField] float torque = 50f;
	[SerializeField] float brakeTorque = 50f;

	[SerializeField] Transform flRenderer;
	[SerializeField] Transform frRenderer;
	[SerializeField] Transform rlRenderer;
	[SerializeField] Transform rrRenderer;

	[SerializeField] WheelCollider flCol;
	[SerializeField] WheelCollider frCol;
	[SerializeField] WheelCollider rlCol;
	[SerializeField] WheelCollider rrCol;

	Vector2 moveInput;
	Vector2 lerpMoveInput;

	private void Update()
	{
		lerpMoveInput = Vector2.Lerp(lerpMoveInput, moveInput, Time.deltaTime * 5f);
		
		SyncWheelRenderer(flCol, flRenderer);
		SyncWheelRenderer(frCol, frRenderer);
		SyncWheelRenderer(rlCol, rlRenderer);
		SyncWheelRenderer(rrCol, rrRenderer);

		Move();
	}

	private void Move()
	{
		float x = lerpMoveInput.x;
		float y = lerpMoveInput.y;
		if (y > 0.1f)
		{
			flCol.brakeTorque = 0f;
			frCol.brakeTorque = 0f;
			rlCol.brakeTorque = 0f;
			rrCol.brakeTorque = 0f;
			rlCol.motorTorque = y * torque;
			rrCol.motorTorque = y * torque;
		}
		else if(y < -0.1f)
		{
			rlCol.motorTorque = 0f;
			rrCol.motorTorque = 0f;
			flCol.brakeTorque = -y * brakeTorque;
			frCol.brakeTorque = -y * brakeTorque;
			rlCol.brakeTorque = -y * brakeTorque;
			rrCol.brakeTorque = -y * brakeTorque;
		}

		flCol.steerAngle = x * rotationAngle;
		frCol.steerAngle = x * rotationAngle;
	}

	private void SyncWheelRenderer(WheelCollider col, Transform target)
	{
		col.GetWorldPose(out Vector3 pos, out Quaternion quat);
		target.SetPositionAndRotation(pos, quat);
	}

	private void OnMove(InputValue value)
	{
		moveInput = value.Get<Vector2>();
	}
}
