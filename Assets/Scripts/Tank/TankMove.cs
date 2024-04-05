using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class TankMove : MonoBehaviour
{
	public enum State { Park, RpmMatch, Drive, ReverseRpmMatch, Reverse, GearShift, }

	[SerializeField] DashBoard dashBoard;
	[SerializeField] AnimationCurve torquePerRpm;
	[SerializeField] float brakeTorque = 2000f;
	[SerializeField] float trackYOffset = 0.2f;
	[SerializeField] float minEngineRpm = 1000f;
	[SerializeField] float maxEngineRpm = 6000f;
	[SerializeField] float sidewayFrictionValue = 2f;
	[SerializeField] float power = 10000f;
	[SerializeField] float inputSensitivity = 3f;
	[SerializeField] float rpmMatchSpeed = 100f;
	[SerializeField] float maxSpeed = 50f;
	[SerializeField] float maxReverseSpeed = -15f;
	[SerializeField] float minRotateRpmDiff = 100f;
	[SerializeField] float maxRotateRpmDiff = 300f;

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

	Vector2 rawMoveInput;
	Vector2 lerpedMoveInput;
	int wheelNum;

	float curMotorTorque;
	NetworkStateMachine stateMachine;
	
	public int Direction { get; private set; }
	public bool Reverse { get; set; }
	public float TorqueMultiplier { get; set; }
	public float MaxTorqueRpm { get; private set; }
	public float RpmMatchSpeed { get { return rpmMatchSpeed; } }
	public int CurGear { get; set; }
	public float[] GearChangeSpeeds { get { return gearChangeSpeeds; } }
	public float CurGearRatio { get { return gears[CurGear]; } }
	public float MinEngineRpm { get { return minEngineRpm; } }
	public float MaxEngineRpm { get { return maxEngineRpm; } }
	public float BrakeMultiplier { get; set; }
	public float Velocity { get; private set; }
	public float AbsWheelRpm { get; set; }
	public float EngineRpm { get; set; }
	public float MaxSpeed {  get { return maxSpeed; } }
	public float MaxReverseSpeed { get { return maxReverseSpeed; } }
	public Vector2 RawMoveInput { get { return rawMoveInput; } }
	public Vector2 LerpedMoveInput { get { return lerpedMoveInput; } }
	public WheelCollider[] LeftWheelCols { get { return leftWheelCols; } }
	public WheelCollider[] RightWheelCols { get { return rightWheelCols; } }
	public Transform[] LeftWheelTrans { get { return leftWheelTrans; } }
	public Transform[] RightWheelTrans { get { return rightWheelTrans; } }

	private void Awake()
	{
		stateMachine = gameObject.AddComponent<NetworkStateMachine>();
		stateMachine.AddState(State.Park, new TankPark(this));
		stateMachine.AddState(State.RpmMatch, new TankRpmMatch(this));
		stateMachine.AddState(State.Drive, new TankDrive(this));
		stateMachine.AddState(State.GearShift, new TankGearShift(this));
		stateMachine.AddState(State.ReverseRpmMatch, new TankReverseRpmMatch(this));
		stateMachine.AddState(State.Reverse, new TankReverse(this));

		stateMachine.InitState(State.Park);

		EngineRpm = minEngineRpm;

		GetMaxTorqueRpm();

		rb = GetComponent<Rigidbody>();
		wheelNum = leftWheelTrans.Length;
	}

	public void SetDashBoardGear(string gearStr)
	{
		dashBoard.SetGearUI(gearStr);
	}

	private void Update()
	{
		lerpedMoveInput = Vector2.Lerp(lerpedMoveInput, rawMoveInput, Time.deltaTime * inputSensitivity);

		AbsWheelRpm = CalculateWheelRPM(out float velocity);

		dashBoard.SetRPMAndVelUI(EngineRpm, velocity);
		this.Velocity = velocity;
		SyncWheelRenderer();
	}

	private void FixedUpdate()
	{
		CalcTorque();
		SetWheelTorque();
		Steering();
	}

	private void GetMaxTorqueRpm()
	{
		Keyframe[] keys = torquePerRpm.keys;
		Keyframe? maxKey = null;
		float maxValue = float.MinValue;
		foreach (Keyframe key in keys)
		{
			if(key.value > maxValue)
			{
				maxValue = key.value;
				maxKey = key;
			}
		}

		if (maxKey == null)
		{
			MaxTorqueRpm = 0f;
			return;
		}

		MaxTorqueRpm = minEngineRpm + (maxKey.Value.time * (maxEngineRpm - minEngineRpm));
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
		Direction = velocity > -0.01f ? 1 : -1;
		return (Mathf.Abs(leftRpm) + Mathf.Abs(rightRpm)) * 0.5f;
	}

	public void SetWheelTorque()
	{
		for(int i = 0; i < wheelNum; i++)
		{
			leftWheelCols[i].brakeTorque = brakeTorque * BrakeMultiplier;
			rightWheelCols[i].brakeTorque = brakeTorque * BrakeMultiplier;
		}

		for(int i = 1; i< wheelNum - 1; i++) 
		{
			leftWheelCols[i].motorTorque = curMotorTorque * TorqueMultiplier;
			rightWheelCols[i].motorTorque = curMotorTorque * TorqueMultiplier;
		}
	}

	private void Steering()
	{
		float xInput = lerpedMoveInput.x;
		FrictionAdjust(xInput);

		float rotateRpmDiff = Mathf.Lerp(minRotateRpmDiff, maxRotateRpmDiff, 30f - Mathf.Abs(Velocity));
		if(Reverse == false)
		{
			float leftTargetRpm = AbsWheelRpm + xInput * rotateRpmDiff;
			float rightTargetRpm = AbsWheelRpm - xInput * rotateRpmDiff;

			if (Mathf.Abs(leftTargetRpm - leftRpm) > 10)
			{
				for (int i = 1; i < wheelNum - 1; i++)
				{
					leftWheelCols[i].motorTorque += leftTargetRpm < leftRpm ? -2000f : 2000f;
				}
			}

			if (Mathf.Abs(rightTargetRpm - rightRpm) > 10)
			{
				for (int i = 1; i < wheelNum - 1; i++)
				{
					rightWheelCols[i].motorTorque += rightTargetRpm < rightRpm ? -2000f : 2000f;
				}
			}
		}
		else
		{
			float leftTargetRpm = -AbsWheelRpm + xInput * rotateRpmDiff;
			float rightTargetRpm = -AbsWheelRpm - xInput * rotateRpmDiff;

			if (Mathf.Abs(leftTargetRpm - leftRpm) > 10)
			{
				for (int i = 1; i < wheelNum - 1; i++)
				{
					leftWheelCols[i].motorTorque += leftTargetRpm < leftRpm ? 2000f : -2000f;
				}
			}

			if (Mathf.Abs(rightTargetRpm - rightRpm) > 10)
			{
				for (int i = 1; i < wheelNum - 1; i++)
				{
					rightWheelCols[i].motorTorque += rightTargetRpm < rightRpm ? 2000f : -2000f;
				}
			}
		}

		leftTorque = leftWheelCols[1].motorTorque;
		rightTorque = rightWheelCols[1].motorTorque;
	}

	private void FrictionAdjust(float xInput)
	{
		int midIdx = 4;
		for (int i = 1; i < wheelNum - 1; i++)
		{
			WheelFrictionCurve sidewayFriction = leftWheelCols[i].sidewaysFriction;
			if (Mathf.Abs(xInput) > 0.1f)
			{
				if (i == midIdx)
					sidewayFriction.stiffness = (Mathf.Abs(xInput)) * sidewayFrictionValue * (wheelNum - 2);
				else
					sidewayFriction.stiffness = Mathf.Clamp01(0.7f - Mathf.Abs(xInput)) * sidewayFrictionValue;
			}
			leftWheelCols[i].sidewaysFriction = sidewayFriction;
			rightWheelCols[i].sidewaysFriction = sidewayFriction;
		}
	}

	public void CalcTorque()
	{
		float torque = torquePerRpm.Evaluate((EngineRpm - minEngineRpm) / maxEngineRpm);
		torque *= gears[CurGear];
		torque /= wheelNum - 2;
		torque *= 2.5f;

		curMotorTorque = torque;
	}

	public void SetEngineRpmWithWheel()
	{
		EngineRpm = AbsWheelRpm * gears[CurGear];
		if(EngineRpm > maxEngineRpm)
		{
			EngineRpm = maxEngineRpm;
		}
	}

	public void SetEngineRpmWithoutWheel(float value)
	{
		EngineRpm = value;
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

	private void OnMove(InputValue value)
	{
		rawMoveInput = value.Get<Vector2>();
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
