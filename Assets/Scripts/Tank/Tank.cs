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
	[SerializeField] float brakeTorque = 2000f;
	[SerializeField] float trackYOffset = 0.2f;
	[SerializeField] float minEngineRpm = 1000f;
	[SerializeField] float maxEngineRpm = 6000f;
	[SerializeField] float sidewayFrictionValue = 2f;
	[SerializeField] float power = 10000f;

	[SerializeField] Transform[] leftWheelTrans;
	[SerializeField] Transform[] rightWheelTrans;

	[SerializeField] WheelCollider[] leftWheelCols;
	[SerializeField] WheelCollider[] rightWheelCols;

	[SerializeField] float[] gears;
	[SerializeField] float[] gearChangeSpeeds;

	[SerializeField] TrackController leftTrack;
	[SerializeField] TrackController rightTrack;

	[Header("Debug")]
	[SerializeField] float leftTorque;
	[SerializeField] float rightTorque;

	Rigidbody rb;

	float rawLeftRpm;
	float rawRightRpm;
	float leftRpm;
	float rightRpm;

	int curGear = 0;

	Vector2 moveInput;
	int wheelNum;

	public WheelCollider[] LeftWheelCols { get { return leftWheelCols; } }
	public WheelCollider[] RightWheelCols { get { return rightWheelCols; } }
	public Transform[] LeftWheelTrans { get { return leftWheelTrans; } }
	public Transform[] RightWheelTrans { get { return rightWheelTrans; } }

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		wheelNum = leftWheelTrans.Length;
	}

	private void Update()
	{
		float wheelRpm = CalculateWheelRPM(out float velocity);
		GearShift(velocity);
		float engineRpm = CalculateEngineRPM(wheelRpm);
		dashBoard.SetRPMAndVelUI(engineRpm, velocity);
		float motorTorque = SetTorque(engineRpm, velocity);
		Steering(motorTorque, velocity);

		SyncWheelRenderer();
	}

	private float CalculateWheelRPM(out float velocity)
	{
		float radios = leftWheelCols[1].radius;

		#region minBaseRpm
		//float measuredLeftRpm = float.MaxValue;
		//float measuredRightRpm = float.MaxValue;
		//for (int i = 1; i < wheelNum - 1; i++)
		//{
		//	measuredLeftRpm = Mathf.Min(measuredLeftRpm, Mathf.Abs(leftWheelCols[i].rpm));
		//}
		//if (leftWheelCols[4].rpm < 0f)
		//{
		//	measuredLeftRpm = -measuredLeftRpm;
		//}


		//for (int i = 1; i < wheelNum - 1; i++)
		//{
		//	measuredRightRpm = Mathf.Min(measuredRightRpm, Mathf.Abs(rightWheelCols[i].rpm));
		//}
		//if (rightWheelCols[4].rpm < 0f)
		//{
		//	measuredRightRpm = -measuredRightRpm;
		//}
		#endregion

		#region avgBaseRpm
		float measuredLeftRpm = 0f;
		float measuredRightRpm = 0f;
		for (int i = 1; i < wheelNum - 1; i++)
		{
			measuredLeftRpm += leftWheelCols[i].rpm;
		}
		measuredLeftRpm /= wheelNum - 2;

		for (int i = 1; i < wheelNum - 1; i++)
		{
			measuredRightRpm += rightWheelCols[i].rpm;
		}
		measuredRightRpm /= wheelNum - 2;
		#endregion

		rawLeftRpm = measuredLeftRpm;
		rawRightRpm = measuredRightRpm;

		leftRpm = Mathf.Lerp(leftRpm, measuredLeftRpm, Time.deltaTime * 2f);
		rightRpm = Mathf.Lerp(rightRpm, measuredRightRpm, Time.deltaTime * 2f);

		leftTrack.Velocity = rawLeftRpm * 60f * 2 * radios * Mathf.PI * 0.001f;
		rightTrack.Velocity = rawRightRpm * 60f * 2 * radios * Mathf.PI * 0.001f;

		float leftVelocity = leftRpm * 60f * 2 * radios * Mathf.PI * 0.001f;
		float rightVelocity = rightRpm * 60f * 2 * radios * Mathf.PI * 0.001f;

		velocity = (leftVelocity + rightVelocity) * 0.5f;

		return (Mathf.Abs(leftRpm) + Mathf.Abs(rightRpm)) * 0.5f;
	}

	private float CalculateEngineRPM(float wheelRpm)
	{
		float gearRatio;
		if(curGear == -1) // P or N
		{
			return minEngineRpm;
		}
		else if(curGear == -2) // R
		{
			gearRatio = gears[0];
		}
		else
		{
			gearRatio = gears[curGear];
		}
		float engineRpm = Mathf.Abs(wheelRpm) * gearRatio;
		return engineRpm;
	}

	private float SetTorque(float engineRpm, float velocity)
	{
		float motorTorque = torquePerRpm.Evaluate((minEngineRpm + engineRpm) / maxEngineRpm);
		motorTorque *= gears[curGear];
		motorTorque /= wheelNum - 2;
		motorTorque *= 3f;

		for (int i = 1; i < wheelNum - 1; i++)
		{

			if (moveInput.y > 0.1f)
			{
				leftWheelCols[i].brakeTorque = 0f;
				rightWheelCols[i].brakeTorque = 0f;
				leftWheelCols[i].motorTorque = motorTorque;
				rightWheelCols[i].motorTorque = motorTorque;
			}
			else
			{
				leftWheelCols[i].motorTorque = 0f;
				rightWheelCols[i].motorTorque = 0f;
				if (moveInput.y < -0.1f)
				{
					leftWheelCols[i].brakeTorque = brakeTorque;
					rightWheelCols[i].brakeTorque = brakeTorque;
				}
			}
		}
		return motorTorque;
	}

	private void Steering(float motorTorque, float velocity)
	{
		float xInput = moveInput.x;

		if (velocity < -0.1f && moveInput.y < -0.1f)
		{
			xInput = -xInput;
		}

		int midIdx = 4;
		for (int i = 1; i < wheelNum - 1; i++)
		{
			if (Mathf.Abs(xInput) > 0.1f)
			{
				WheelFrictionCurve sidewayFriction = leftWheelCols[i].sidewaysFriction;
				if (i == midIdx)
					sidewayFriction.stiffness = 14f;
				else
					sidewayFriction.stiffness = 0f;
				leftWheelCols[i].sidewaysFriction = sidewayFriction;
				rightWheelCols[i].sidewaysFriction = sidewayFriction;
			}
			else
			{
				WheelFrictionCurve sidewayFriction = leftWheelCols[i].sidewaysFriction;
				sidewayFriction.stiffness = sidewayFrictionValue;
				leftWheelCols[i].sidewaysFriction = sidewayFriction;
				rightWheelCols[i].sidewaysFriction = sidewayFriction;
			}
		}

		for (int i = 1; i < wheelNum - 1; i++)
		{
			leftWheelCols[i].motorTorque += xInput * motorTorque * 0.5f;
			rightWheelCols[i].motorTorque -= xInput * motorTorque * 0.5f;
		}

		leftTorque = leftWheelCols[1].motorTorque;
		rightTorque = rightWheelCols[1].motorTorque;
	}

	private void SyncWheelRenderer()
	{
		float leftRps = rawLeftRpm / 60f;
		float rightRps = rawRightRpm / 60f;

		for (int i = 0; i < wheelNum; i++)
		{
			leftWheelTrans[i].Rotate(360f * leftRps * Time.deltaTime, 0f, 0f);
			rightWheelTrans[i].Rotate(360f * rightRps * Time.deltaTime, 0f, 0f);
		}

		for (int i = 1; i < wheelNum - 1; i++)
		{
			leftWheelCols[i].GetWorldPose(out Vector3 pos, out Quaternion rot);
			leftWheelTrans[i].position = pos;
			Vector3 localPos = leftWheelTrans[i].localPosition;
			localPos.y += trackYOffset;
			leftWheelTrans[i].localPosition = localPos;

			rightWheelCols[i].GetWorldPose(out pos, out rot);
			rightWheelTrans[i].position = pos;
			localPos = rightWheelTrans[i].localPosition;
			localPos.y += trackYOffset;
			rightWheelTrans[i].localPosition = localPos;
		}
	}

	private void GearShift(float velocity)
	{
		int prevGear = curGear;
		if(gears.Length - 1 > curGear && velocity > gearChangeSpeeds[curGear])
		{
			curGear++;
		}
		else if(curGear > 0 && velocity < gearChangeSpeeds[curGear - 1] - 5f )
		{
			curGear--;
		}


		if (prevGear == curGear) return;


		if(curGear >= 0)
			dashBoard.SetGearUI((curGear + 1).ToString());
		else if(curGear == -1)
			dashBoard.SetGearUI("P");
		else if(curGear == -2)
			dashBoard.SetGearUI("R");
	}


	private void OnMove(InputValue value)
	{
		moveInput = value.Get<Vector2>();
	}

	private void OnFire(InputValue value)
	{
		bool pressed = value.Get<float>() > 0.9f;

		if(pressed)
		{
			rb.AddForceAtPosition(transform.up * power, transform.position + transform.forward * 2f + transform.up, ForceMode.Impulse);
		}
	}
}
