using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using Fusion;
using Cinemachine;
using Fusion.Addons.Physics;

public class TankMove : VehicleMove
{
	[SerializeField] float sidewayFrictionValue = 2f;
	[SerializeField] float minRotateRpmDiff = 100f;
	[SerializeField] float maxRotateRpmDiff = 300f;

	[SerializeField] TrackController leftTrack;
	[SerializeField] TrackController rightTrack;

	[SerializeField] Transform[] leftRollers;
	[SerializeField] Transform[] rightRollers;

	[Header("Debug")]
	[SerializeField] float leftTorque;
	[SerializeField] float rightTorque;


	//Vector2 lerpedMoveInput;


	public override void Spawned()
	{
		base.Spawned();

		leftTrack.gameObject.SetActive(true);
		rightTrack.gameObject.SetActive(true);
	}

	protected override void SyncWheelRenderer(float leftRps, float rightRps)
	{
		base.SyncWheelRenderer(leftRps, rightRps);

		for (int i = 0; i < leftRollers.Length; i++)
		{
			leftRollers[i].Rotate(360f * leftRps * Runner.DeltaTime, 0f, 0f);
			rightRollers[i].Rotate(360f * rightRps * Runner.DeltaTime, 0f, 0f);
		}
	}

	protected override void Steering()
	{
		const float RPM_DIFF = 10f;
		float torqueAdder = 2000f;
		float xInput = moveInput.x;
		FrictionAdjust(xInput);

		float rotateRpmDiff = Mathf.Lerp(minRotateRpmDiff, maxRotateRpmDiff, 30f - Mathf.Abs(Velocity));
		if (Reverse == false)
		{
			float leftTargetRpm = AbsWheelRpm + xInput * rotateRpmDiff;
			float rightTargetRpm = AbsWheelRpm - xInput * rotateRpmDiff;

			if (Mathf.Abs(leftTargetRpm - RawLeftRpm) > RPM_DIFF)
			{
				for (int i = 0; i < wheelNum; i++)
				{
					LeftWheelCols[i].motorTorque += leftTargetRpm < RawLeftRpm ? -torqueAdder : torqueAdder;
				}
			}

			if (Mathf.Abs(rightTargetRpm - RawRightRpm) > RPM_DIFF)
			{
				for (int i = 0; i < wheelNum; i++)
				{
					RightWheelCols[i].motorTorque += rightTargetRpm < RawRightRpm ? -torqueAdder : torqueAdder;
				}
			}
		}
		else
		{
			float leftTargetRpm = -AbsWheelRpm + xInput * rotateRpmDiff;
			float rightTargetRpm = -AbsWheelRpm - xInput * rotateRpmDiff;

			if (Mathf.Abs(leftTargetRpm - RawLeftRpm) > RPM_DIFF)
			{
				for (int i = 0; i < wheelNum; i++)
				{
					LeftWheelCols[i].motorTorque += leftTargetRpm < RawLeftRpm ? torqueAdder : -torqueAdder;
				}
			}

			if (Mathf.Abs(rightTargetRpm - RawRightRpm) > RPM_DIFF)
			{
				for (int i = 0; i < wheelNum; i++)
				{
					RightWheelCols[i].motorTorque += rightTargetRpm < RawRightRpm ? torqueAdder : -torqueAdder;
				}
			}
		}

		leftTorque = LeftWheelCols[0].motorTorque;
		rightTorque = RightWheelCols[0].motorTorque;
	}

	private void FrictionAdjust(float xInput)
	{
		int midIdx = 3;
		for (int i = 0; i < wheelNum; i++)
		{
			WheelFrictionCurve sidewayFriction = LeftWheelCols[i].sidewaysFriction;
			if (Mathf.Abs(xInput) > 0.1f)
			{
				if (i == midIdx)
					sidewayFriction.stiffness = (Mathf.Abs(xInput)) * sidewayFrictionValue * (wheelNum);
				else
					sidewayFriction.stiffness = Mathf.Clamp01(0.7f - Mathf.Abs(xInput)) * sidewayFrictionValue;
			}
			LeftWheelCols[i].sidewaysFriction = sidewayFriction;
			RightWheelCols[i].sidewaysFriction = sidewayFriction;
		}
	}
}