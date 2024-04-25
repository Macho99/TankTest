using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using Fusion;
using Cinemachine;
using Fusion.Addons.Physics;

public class TankMove : NetworkBehaviour
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
	[SerializeField] float torqueMultiplier = 2.5f;

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


	Vector2 moveInput;
	//Vector2 lerpedMoveInput;
	int wheelNum;

	NetworkStateMachine stateMachine;
	NetworkRigidbody3D netRb;
	
	public int Direction { get; private set; }
	public float MaxTorqueRpm { get; private set; }
	public float RpmMatchSpeed { get { return rpmMatchSpeed; } }
	public float[] GearChangeSpeeds { get { return gearChangeSpeeds; } }
	public float CurGearRatio { get { return gears[CurGear]; } }
	public float MinEngineRpm { get { return minEngineRpm; } }
	public float MaxEngineRpm { get { return maxEngineRpm; } }
	public float EngineRpm { get; set; }
	public float MaxSpeed {  get { return maxSpeed; } }
	public float MaxReverseSpeed { get { return maxReverseSpeed; } }
	public Vector2 MoveInput { get { return moveInput; } }
	public WheelCollider[] LeftWheelCols { get { return leftWheelCols; } }
	public WheelCollider[] RightWheelCols { get { return rightWheelCols; } }
	public Transform[] LeftWheelTrans { get { return leftWheelTrans; } }
	public Transform[] RightWheelTrans { get { return rightWheelTrans; } }


	[Networked, HideInInspector] public float TorqueMultiplier { get; set; }
	[Networked, HideInInspector] public float BrakeMultiplier { get; set; }
	[Networked, HideInInspector] public int CurGear { get; set; }
	[Networked, HideInInspector] public float LeftRpm { get; private set; }
	[Networked, HideInInspector] public float RightRpm { get; private set; }
	[Networked, HideInInspector] public float RawLeftRpm { get; private set; }
	[Networked, HideInInspector] public float RawRightRpm { get; private set; }
	[Networked, HideInInspector] public float AbsWheelRpm { get; set; }
	[Networked, HideInInspector] public float Velocity { get; private set; }
	[Networked, HideInInspector] public float CurMotorTorque { get; private set; }
	[Networked, HideInInspector] public NetworkBool Reverse { get; set; }

	private void Awake()
	{
		netRb = GetComponent<NetworkRigidbody3D>();

		stateMachine = GetComponent<NetworkStateMachine>();
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

	public override void Spawned()
	{
		if(Object.HasInputAuthority == false)
		{
			transform.parent.GetComponentInChildren<Canvas>().gameObject.SetActive(false);
		}
		leftTrack.gameObject.SetActive(true);
		rightTrack.gameObject.SetActive(true);
	}

	public void SetDashBoardGear(string gearStr)
	{
		dashBoard.SetGearUI(gearStr);
	}

	public override void Render()
	{
		//lerpedMoveInput = Vector2.Lerp(lerpedMoveInput, rawMoveInput, Runner.DeltaTime * inputSensitivity);

		dashBoard.SetRPMAndVelUI(EngineRpm, Velocity);
		SyncWheelRenderer();
	}

	public override void FixedUpdateNetwork()
	{
		if(GetInput(out TestInputData input) == true)
		{
			moveInput = input.moveVec;
		}
		else
		{
			moveInput = Vector2.zero;
		}
		//if(input.buttons.IsSet(Buttons.Respawn))
		//{
		//	netRb.Teleport(GameObject.FindGameObjectWithTag("Respawn").transform.position);
		//}

		AbsWheelRpm = CalculateWheelRPM(out float velocity);
		this.Velocity = velocity;
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

		RawLeftRpm = measuredLeftRpm;
		RawRightRpm = measuredRightRpm;

		LeftRpm = Mathf.Lerp(LeftRpm, measuredLeftRpm, Runner.DeltaTime * 2f);
		RightRpm = Mathf.Lerp(RightRpm, measuredRightRpm, Runner.DeltaTime * 2f);

		//leftTrack.Velocity = RawLeftRpm * 60f * 2 * radios * Mathf.PI * 0.001f;
		//rightTrack.Velocity = RawRightRpm * 60f * 2 * radios * Mathf.PI * 0.001f;

		float leftVelocity = LeftRpm * 60f * 2 * radios * Mathf.PI * 0.001f;
		float rightVelocity = RightRpm * 60f * 2 * radios * Mathf.PI * 0.001f;

		velocity = (leftVelocity + rightVelocity) * 0.5f;
		Direction = velocity > -0.01f ? 1 : -1;
		return (Mathf.Abs(LeftRpm) + Mathf.Abs(RightRpm)) * 0.5f;
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
			leftWheelCols[i].motorTorque = CurMotorTorque * TorqueMultiplier;
			rightWheelCols[i].motorTorque = CurMotorTorque * TorqueMultiplier;
		}
	}

	private void Steering()
	{
		float xInput = moveInput.x;
		FrictionAdjust(xInput);

		float rotateRpmDiff = Mathf.Lerp(minRotateRpmDiff, maxRotateRpmDiff, 30f - Mathf.Abs(Velocity));
		if(Reverse == false)
		{
			float leftTargetRpm = AbsWheelRpm + xInput * rotateRpmDiff;
			float rightTargetRpm = AbsWheelRpm - xInput * rotateRpmDiff;

			if (Mathf.Abs(leftTargetRpm - LeftRpm) > 10)
			{
				for (int i = 1; i < wheelNum - 1; i++)
				{
					leftWheelCols[i].motorTorque += leftTargetRpm < LeftRpm ? -2000f : 2000f;
				}
			}

			if (Mathf.Abs(rightTargetRpm - RightRpm) > 10)
			{
				for (int i = 1; i < wheelNum - 1; i++)
				{
					rightWheelCols[i].motorTorque += rightTargetRpm < RightRpm ? -2000f : 2000f;
				}
			}
		}
		else
		{
			float leftTargetRpm = -AbsWheelRpm + xInput * rotateRpmDiff;
			float rightTargetRpm = -AbsWheelRpm - xInput * rotateRpmDiff;

			if (Mathf.Abs(leftTargetRpm - LeftRpm) > 10)
			{
				for (int i = 1; i < wheelNum - 1; i++)
				{
					leftWheelCols[i].motorTorque += leftTargetRpm < LeftRpm ? 2000f : -2000f;
				}
			}

			if (Mathf.Abs(rightTargetRpm - RightRpm) > 10)
			{
				for (int i = 1; i < wheelNum - 1; i++)
				{
					rightWheelCols[i].motorTorque += rightTargetRpm < RightRpm ? 2000f : -2000f;
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
		torque *= torqueMultiplier;

		CurMotorTorque = torque;
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
		float leftRps = RawLeftRpm / 60f;
		float rightRps = RawRightRpm / 60f;

		for (int i = 0; i < wheelNum; i++)
		{
			leftWheelTrans[i].Rotate(360f * leftRps * Runner.DeltaTime, 0f, 0f);
			rightWheelTrans[i].Rotate(360f * rightRps * Runner.DeltaTime, 0f, 0f);
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
}
