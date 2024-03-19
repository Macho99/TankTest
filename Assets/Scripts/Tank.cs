using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tank : MonoBehaviour
{
	[SerializeField] float rotationAngle = 35f;
	[SerializeField] float torque = 2000f;
	[SerializeField] float brakeTorque = 10000f;
	[SerializeField] float trackYOffset = 0.2f;

	[SerializeField] Transform[] leftRenderers;
	[SerializeField] Transform[] rightRenderers;

	[SerializeField] WheelCollider[] leftWheelCols;
	[SerializeField] WheelCollider[] rightWheelCols;

	[Header("Debug")]
	[SerializeField] float leftTorque;
	[SerializeField] float rightTorque;

	Vector2 moveInput;
	Vector2 lerpMoveInput;
	int wheelNum;

	private void Awake()
	{
		wheelNum = leftRenderers.Length;
	}

	private void Update()
	{
		lerpMoveInput = Vector2.Lerp(lerpMoveInput, moveInput, Time.deltaTime * 5f);

		Move();

		for (int i = 0; i < wheelNum; i++)
		{
			SyncWheelRenderer(leftWheelCols[i], leftRenderers[i]);
			SyncWheelRenderer(rightWheelCols[i], rightRenderers[i]);
		}
	}

	private void Move()
	{
		if(lerpMoveInput.sqrMagnitude > 0.1f)
		{
			float x = lerpMoveInput.x;
			float y = lerpMoveInput.y;

			for (int i = 0; i < wheelNum; i++)
			{
				leftWheelCols[i].brakeTorque = 0f;
				rightWheelCols[i].brakeTorque = 0f;
				leftWheelCols[i].motorTorque = (y + x) * torque;
				rightWheelCols[i].motorTorque = (y - x) * torque;
			}
		}
		else
		{
			for (int i = 0; i < wheelNum; i++)
			{
				leftWheelCols[i].motorTorque = 0f;
				rightWheelCols[i].motorTorque = 0f;
				leftWheelCols[i].brakeTorque = brakeTorque;
				rightWheelCols[i].brakeTorque = brakeTorque;
			}
		}
		leftTorque = leftWheelCols[0].motorTorque;
		rightTorque = rightWheelCols[0].motorTorque;
	}

	private void SyncWheelRenderer(WheelCollider col, Transform target)
	{
		col.GetWorldPose(out Vector3 pos, out Quaternion quat);
		pos.y += trackYOffset;
		target.SetPositionAndRotation(pos, quat);
	}

	private void OnMove(InputValue value)
	{
		moveInput = value.Get<Vector2>();
	}
}
