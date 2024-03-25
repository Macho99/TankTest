using Palmmedia.ReportGenerator.Core.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankMove : MonoBehaviour
{
	public enum State { Neutral, RPMMatch, Drive, Reverse, GearShift, Park, }

	[SerializeField] DashBoard dashBoard;
	[SerializeField] AnimationCurve torquePerRpm;
	[SerializeField] float rotationAngle = 35f;
	[SerializeField] float brakeTorque = 2000f;
	[SerializeField] float trackYOffset = 0.2f;
	[SerializeField] float minEngineRpm = 1000f;
	[SerializeField] float maxEngineRpm = 6000f;
	[SerializeField] float sidewayFrictionValue = 2f;
	[SerializeField] float power = 10000f;
	[SerializeField] float inputSensitivity = 3f;
	[SerializeField] float rpmMatchSpeed = 100f;

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
	StateMachine stateMachine;

	
	public float maxTorqueRpm { get; private set; }
	public float RpmMatchSpeed { get { return rpmMatchSpeed; } }
	public int CurGear { get; private set; }
	public float CurGearRatio { get { return gears[CurGear]; } }
	public float MinEngineRpm { get { return MinEngineRpm; } }
	public float BrakeMultiplier { get; set; }
	public float WheelRpm { get; set; }
	public float EngineRpm { get; set; }
	public Vector2 RawMoveInput { get { return rawMoveInput; } }
	public Vector2 LerpedMoveInput { get { return lerpedMoveInput; } }
	public WheelCollider[] LeftWheelCols { get { return leftWheelCols; } }
	public WheelCollider[] RightWheelCols { get { return rightWheelCols; } }
	public Transform[] LeftWheelTrans { get { return leftWheelTrans; } }
	public Transform[] RightWheelTrans { get { return rightWheelTrans; } }

	private void Awake()
	{
		stateMachine = gameObject.AddComponent<StateMachine>();
		stateMachine.AddState(State.Park, new TankPark(this));
		stateMachine.AddState(State.Neutral, new TankNeutral(this));
		stateMachine.AddState(State.RPMMatch, new TankRpmMatch(this));

		stateMachine.InitState(State.Park);

		EngineRpm = minEngineRpm;

		GetMaxTorqueRpm();

		rb = GetComponent<Rigidbody>();
		wheelNum = leftWheelTrans.Length;
	}

	private void Update()
	{
		lerpedMoveInput = Vector2.Lerp(lerpedMoveInput, rawMoveInput, Time.deltaTime * inputSensitivity);

		WheelRpm = CalculateWheelRPM(out float velocity);
		//GearShift(velocity);
		//EngineRpm = CalculateEngineRPM(WheelRpm);
		//float motorTorque = SetTorque(EngineRpm, velocity);
		//Steering(motorTorque, velocity);

		dashBoard.SetRPMAndVelUI(EngineRpm, velocity);
		SyncWheelRenderer();
	}

	private void FixedUpdate()
	{
		CalcTorque();
		SetWheelTorque();
	}

	private void GetMaxTorqueRpm()
	{
		Keyframe[] keys = torquePerRpm.keys;
		Keyframe? maxKey = null;
		float maxValue = float.MinValue;
		foreach (Keyframe key in keys)
		{

		}
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

	public void SetWheelTorque()
	{
		for(int i = 0; i < wheelNum; i++)
		{
			leftWheelCols[i].brakeTorque = brakeTorque * BrakeMultiplier;
			rightWheelCols[i].brakeTorque = brakeTorque * BrakeMultiplier;
		}

		for(int i = 1; i< wheelNum - 1; i++) 
		{
			leftWheelCols[i].motorTorque = curMotorTorque;
			rightWheelCols[i].motorTorque = curMotorTorque;
		}
	}

	public void CalcTorque()
	{
		float torque = torquePerRpm.Evaluate(EngineRpm / maxEngineRpm);
		torque *= gears[CurGear];
		torque /= wheelNum - 2;
		torque *= 3f;

		curMotorTorque = torque;
	}

	public void SetEngineRpmWithWheel()
	{
		EngineRpm = WheelRpm * gears[CurGear];
	}

	public void SetEngineRpmWithoutWheel(float value)
	{
		EngineRpm = value;
	}

	//private float SetTorque(float engineRpm, float velocity)
	//{
	//	curMotorTorque = torquePerRpm.Evaluate((minEngineRpm + engineRpm) / maxEngineRpm);
	//	curMotorTorque *= gears[curGear];
	//	curMotorTorque /= wheelNum - 2;
	//	curMotorTorque *= 3f;

	//	for (int i = 1; i < wheelNum - 1; i++)
	//	{
	//		if (rawMoveInput.y > 0.1f)
	//		{
	//			leftWheelCols[i].brakeTorque = 0f;
	//			rightWheelCols[i].brakeTorque = 0f;
	//			leftWheelCols[i].motorTorque = curMotorTorque;
	//			rightWheelCols[i].motorTorque = curMotorTorque;
	//		}
	//		else
	//		{
	//			leftWheelCols[i].motorTorque = 0f;
	//			rightWheelCols[i].motorTorque = 0f;
	//			if (rawMoveInput.y < -0.1f)
	//			{
	//				leftWheelCols[i].brakeTorque = brakeTorque;
	//				rightWheelCols[i].brakeTorque = brakeTorque;
	//			}
	//		}
	//	}
	//	return curMotorTorque;
	//}

	private void Steering(float motorTorque, float velocity)
	{
		float xInput = rawMoveInput.x;

		if (velocity < -0.1f && rawMoveInput.y < -0.1f)
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
		int prevGear = CurGear;
		if(gears.Length - 1 > CurGear && velocity > gearChangeSpeeds[CurGear])
		{
			CurGear++;
		}
		else if(CurGear > 0 && velocity < gearChangeSpeeds[CurGear - 1] - 5f )
		{
			CurGear--;
		}


		if (prevGear == CurGear) return;


		if(CurGear >= 0)
			dashBoard.SetGearUI((CurGear + 1).ToString());
		else if(CurGear == -1)
			dashBoard.SetGearUI("P");
		else if(CurGear == -2)
			dashBoard.SetGearUI("R");
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
