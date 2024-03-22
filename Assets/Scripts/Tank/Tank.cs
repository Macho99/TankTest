using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tank : MonoBehaviour
{
	[SerializeField] DashBoard dashBoard;
	[SerializeField] AnimationCurve torquePerRpm;
	[SerializeField] float rotationAngle = 35f;
	[SerializeField] float torque = 2000f;
	[SerializeField] float brakeTorque = 2000f;
	[SerializeField] float trackYOffset = 0.2f;

	[SerializeField] Transform[] leftWheelTrans;
	[SerializeField] Transform[] rightWheelTrans;

	[SerializeField] WheelCollider[] leftWheelCols;
	[SerializeField] WheelCollider[] rightWheelCols;

	[SerializeField] float[] gears;
	float[] gearChangeSpeed;

	[SerializeField] TrackController leftTrack;
	[SerializeField] TrackController rightTrack;

	[Header("Debug")]
	[SerializeField] float leftTorque;
	[SerializeField] float rightTorque;

	float leftRpm;
	float rightRpm;
	Vector2 moveInput;
	Vector2 lerpMoveInput;
	int wheelNum;

	public WheelCollider[] LeftWheelCols { get { return leftWheelCols; } }
	public WheelCollider[] RightWheelCols { get { return rightWheelCols; } }
	public Transform[] LeftWheelTrans { get { return leftWheelTrans; } }
	public Transform[] RightWheelTrans { get { return rightWheelTrans; } }

	private void Awake()
	{
		gearChangeSpeed = new float[gears.Length];
		wheelNum = leftWheelTrans.Length;
	}

	private void Update()
	{
		lerpMoveInput = Vector2.Lerp(lerpMoveInput, moveInput, Time.deltaTime * 5f);

		Move();
		float wheelRpm = CalculateWheelRPM(out float velocity);
		//float engineRpm = CalculateEngineRPM(wheelRpm, );
		dashBoard.SetRPMAndVelUI(wheelRpm, velocity);

		for (int i = 1; i < wheelNum - 1; i++)
		{
			SyncWheelRenderer(leftWheelCols[i], leftWheelTrans[i]);
			SyncWheelRenderer(rightWheelCols[i], rightWheelTrans[i]);
		}
	}

	private float CalculateWheelRPM(out float velocity)
	{
		float radios = leftWheelCols[1].radius;


		float measuredLeftRpm = float.MaxValue;
		float measuredRightRpm = float.MaxValue;
		for (int i = 1; i < wheelNum - 1; i++)
		{
			measuredLeftRpm = Mathf.Min(measuredLeftRpm, leftWheelCols[i].rpm);
		}

		for (int i = 1; i < wheelNum - 1; i++)
		{
			measuredRightRpm = Mathf.Min(measuredRightRpm, rightWheelCols[i].rpm);
		}

		//float measuredLeftRpm = 0f;
		//float measuredRightRpm = 0f;
		//for (int i = 1; i < wheelNum - 1; i++)
		//{
		//	measuredLeftRpm += leftWheelCols[i].rpm;
		//}
		//measuredLeftRpm /= wheelNum - 2;

		//for (int i = 1; i < wheelNum - 1; i++)
		//{
		//	measuredRightRpm += rightWheelCols[i].rpm;
		//}
		//measuredRightRpm /= wheelNum - 2;

		leftRpm = Mathf.Lerp(leftRpm, measuredLeftRpm, Time.deltaTime * 2f);
		rightRpm = Mathf.Lerp(rightRpm, measuredRightRpm, Time.deltaTime * 2f);

		float leftVelocity = leftRpm * 60f * 2 * radios * Mathf.PI * 0.001f;
		leftTrack.Velocity = leftVelocity;

		float rightVelocity = rightRpm * 60f * 2 * radios * Mathf.PI * 0.001f;
		rightTrack.Velocity = rightVelocity;

		velocity = (leftVelocity + rightVelocity) * 0.5f;

		return (leftRpm + rightRpm) * 0.5f;
	}

	private void Move()
	{
		if (moveInput.sqrMagnitude > 0.1f)
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
			float brakeMultiplier = Mathf.Max(lerpMoveInput.x, lerpMoveInput.y);
			brakeMultiplier = 1 - brakeMultiplier;
			for (int i = 0; i < wheelNum; i++)
			{
				leftWheelCols[i].motorTorque = 0f;
				rightWheelCols[i].motorTorque = 0f;
				leftWheelCols[i].brakeTorque = brakeTorque * brakeMultiplier;
				rightWheelCols[i].brakeTorque = brakeTorque * brakeMultiplier;
			}
		}
		leftTorque = leftWheelCols[0].motorTorque * lerpMoveInput.y;
		rightTorque = rightWheelCols[0].motorTorque * lerpMoveInput.y;
	}

	private void SyncWheelRenderer(WheelCollider col, Transform target)
	{
		col.GetWorldPose(out Vector3 pos, out Quaternion quat);
		target.SetPositionAndRotation(pos, quat);
		Vector3 localPos = target.localPosition;
		localPos.y += trackYOffset;
		target.localPosition = localPos;
	}

	private void OnMove(InputValue value)
	{
		moveInput = value.Get<Vector2>();
	}
}
