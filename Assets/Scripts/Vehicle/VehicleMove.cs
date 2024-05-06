using Fusion.Addons.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Fusion;

public class VehicleMove : VehicleBehaviour
{
	public enum State { Park, RpmMatch, Drive, ReverseRpmMatch, Reverse, GearShift, }

	[SerializeField] float wheelYOffset = 0.2f;
	[SerializeField] float brakeTorque = 2000f;
	[SerializeField] DashBoard dashBoardPrefab;
	[SerializeField] AnimationCurve torquePerRpm;
	[SerializeField] float minEngineRpm = 1000f;
	[SerializeField] float maxEngineRpm = 6000f;
	[SerializeField] float rpmMatchSpeed = 100f;
	[SerializeField] float maxSpeed = 50f;
	[SerializeField] float maxReverseSpeed = -15f;
	[SerializeField] float finalTorqueMultiplier = 2.5f;
	[SerializeField] float steerAngle = 40f;
	
	[SerializeField] Transform[] leftWheelTrans;
	[SerializeField] Transform[] rightWheelTrans;

	[SerializeField] WheelCollider[] leftWheelCols;
	[SerializeField] WheelCollider[] rightWheelCols;

	[SerializeField] float[] gears;
	[SerializeField] float[] gearChangeSpeeds;

	Rigidbody rb;
	DashBoard dashBoard;

	NetworkStateMachine stateMachine;
	NetworkRigidbody3D netRb;

	protected int wheelNum;
	protected Vector2 moveInput;

	public int Direction { get; private set; }
	public float MaxTorqueRpm { get; private set; }
	public float RpmMatchSpeed { get { return rpmMatchSpeed; } }
	public float[] GearChangeSpeeds { get { return gearChangeSpeeds; } }
	public float CurGearRatio { get { return gears[CurGear]; } }
	public float MinEngineRpm { get { return minEngineRpm; } }
	public float MaxEngineRpm { get { return maxEngineRpm; } }
	public float EngineRpm { get; set; }
	public float MaxSpeed { get { return maxSpeed; } }
	public float MaxReverseSpeed { get { return maxReverseSpeed; } }
	public Vector2 MoveInput { get { return moveInput; } }
	public WheelCollider[] LeftWheelCols { get { return leftWheelCols; } }
	public WheelCollider[] RightWheelCols { get { return rightWheelCols; } }
	public Transform[] LeftWheelTrans { get { return leftWheelTrans; } }
	public Transform[] RightWheelTrans { get { return rightWheelTrans; } }
	public NetworkBool Reverse { get; set; }
	public float AbsWheelRpm { get; private set; }
	public float Velocity { get; private set; }
	public float CurMotorTorque { get; private set; }
	public float TorqueMultiplier { get; set; }
	public float BrakeMultiplier { get; set; }
	public int CurGear { get; set; }

	[Networked] public Vector2 LerpedMoveInput { get; set; }
	[Networked] public float LeftRpm { get; private set; }
	[Networked] public float RightRpm { get; private set; }
	[Networked] public float RawLeftRpm { get; private set; }
	[Networked] public float RawRightRpm { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		netRb = GetComponent<NetworkRigidbody3D>();

		stateMachine = GetComponent<NetworkStateMachine>();
		stateMachine.AddState(State.Park, new VehiclePark(this));
		stateMachine.AddState(State.RpmMatch, new VehicleRpmMatch(this));
		stateMachine.AddState(State.Drive, new VehicleDrive(this));
		stateMachine.AddState(State.GearShift, new VehicleGearShift(this));
		stateMachine.AddState(State.ReverseRpmMatch, new VehicleReverseRpmMatch(this));
		stateMachine.AddState(State.Reverse, new VehicleReverse(this));

		stateMachine.InitState(State.Park);

		EngineRpm = minEngineRpm;

		GetMaxTorqueRpm();

		rb = GetComponent<Rigidbody>();
		wheelNum = leftWheelTrans.Length;
	}

	public override void Spawned()
	{
		base.Spawned();
		//if (Object.HasInputAuthority == false)
		//{
		//	transform.parent.GetComponentInChildren<Canvas>(true)?.gameObject.SetActive(false);
		//}
	}

	public void SetDashBoardGear(string gearStr)
	{
		dashBoard?.SetGearUI(gearStr);
	}

	public override void Render()
	{
		base.Render();
		//lerpedMoveInput = Vector2.Lerp(lerpedMoveInput, rawMoveInput, Runner.DeltaTime * inputSensitivity);

		dashBoard?.SetRPMAndVelUI(EngineRpm, Velocity);

		float leftRps = RawLeftRpm / 60f;
		float rightRps = RawRightRpm / 60f;
		SyncWheelRenderer(leftRps, rightRps);
	}

	public override void FixedUpdateNetwork()
	{
		base.FixedUpdateNetwork();
		if (GetInput(out TestInputData input) == true)
		{
			moveInput = input.moveVec;
		}
		else
		{
			moveInput = Vector2.zero;
		}
		if (Runner.IsForward)
		{
			LerpedMoveInput = Vector2.Lerp(LerpedMoveInput, moveInput, Runner.DeltaTime * 2f);
		}

		//if(input.buttons.IsSet(Buttons.Respawn))
		//{
		//	netRb.Teleport(GameObject.FindGameObjectWithTag("Respawn").transform.position);
		//}

		CalculateWheelRPM();
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
			if (key.value > maxValue)
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

	private void CalculateWheelRPM()
	{
		float radios = leftWheelCols[0].radius * transform.localScale.x;

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
		for (int i = 0; i < wheelNum; i++)
		{
			measuredLeftRpm += leftWheelCols[i].rpm;
		}
		measuredLeftRpm /= wheelNum;

		for (int i = 0; i < wheelNum; i++)
		{
			measuredRightRpm += rightWheelCols[i].rpm;
		}
		measuredRightRpm /= wheelNum;
		#endregion

		RawLeftRpm = measuredLeftRpm;
		RawRightRpm = measuredRightRpm;

		LeftRpm = Mathf.Lerp(LeftRpm, measuredLeftRpm, Runner.DeltaTime * 2f);
		RightRpm = Mathf.Lerp(RightRpm, measuredRightRpm, Runner.DeltaTime * 2f);

		float leftVelocity = LeftRpm * 60f * 2 * radios * Mathf.PI * 0.001f;
		float rightVelocity = RightRpm * 60f * 2 * radios * Mathf.PI * 0.001f;

		Velocity = (leftVelocity + rightVelocity) * 0.5f;
		Direction = Velocity > -0.01f ? 1 : -1;
		AbsWheelRpm = (Mathf.Abs(LeftRpm) + Mathf.Abs(RightRpm)) * 0.5f;
	}

	public void SetWheelTorque()
	{
		for (int i = 0; i < wheelNum; i++)
		{
			leftWheelCols[i].brakeTorque = brakeTorque * BrakeMultiplier;
			rightWheelCols[i].brakeTorque = brakeTorque * BrakeMultiplier;
		}

		for (int i = 0; i < wheelNum; i++)
		{
			leftWheelCols[i].motorTorque = CurMotorTorque * TorqueMultiplier;
			rightWheelCols[i].motorTorque = CurMotorTorque * TorqueMultiplier;
		}
	}

	protected virtual void Steering()
	{
		leftWheelCols[0].steerAngle = LerpedMoveInput.x * steerAngle;
		rightWheelCols[0].steerAngle = LerpedMoveInput.x * steerAngle;
	}


	public void CalcTorque()
	{
		float torque = torquePerRpm.Evaluate((EngineRpm - minEngineRpm) / maxEngineRpm);
		torque *= gears[CurGear];
		torque /= wheelNum;
		torque *= finalTorqueMultiplier;

		CurMotorTorque = torque;
	}

	public void SetEngineRpmWithWheel()
	{
		EngineRpm = AbsWheelRpm * gears[CurGear];
		if (EngineRpm > maxEngineRpm)
		{
			EngineRpm = maxEngineRpm;
		}
	}

	public void SetEngineRpmWithoutWheel(float value)
	{
		EngineRpm = value;
	}

	protected virtual void SyncWheelRenderer(float leftRps, float rightRps)
	{
		Vector3 leftEuler = leftWheelTrans[0].localRotation.eulerAngles;
		leftEuler.y = LerpedMoveInput.x * steerAngle + leftEuler.z;
		leftWheelTrans[0].localRotation = Quaternion.Euler(leftEuler);

		Vector3 rightEuler = rightWheelTrans[0].localRotation.eulerAngles;
		rightEuler.y = LerpedMoveInput.x * steerAngle + rightEuler.z;
		rightWheelTrans[0].localRotation = Quaternion.Euler(rightEuler);

		for (int i = 0; i < wheelNum; i++)
		{
			leftWheelTrans[i].Rotate(Vector3.right, 360f * leftRps * Time.deltaTime);
			rightWheelTrans[i].Rotate(Vector3.right, 360f * rightRps * Time.deltaTime);
		}

		for (int i = 0; i < wheelNum; i++)
		{
			leftWheelCols[i].GetWorldPose(out Vector3 posLeft, out Quaternion rotLeft);
			leftWheelTrans[i].localPosition = leftWheelTrans[i].parent.InverseTransformPoint(posLeft) + Vector3.up * wheelYOffset;

			rightWheelCols[i].GetWorldPose(out Vector3 posRight, out Quaternion rotRight);
			rightWheelTrans[i].localPosition = rightWheelTrans[i].parent.InverseTransformPoint(posRight) + Vector3.up * wheelYOffset;
		}
	}

	protected override void OnAssign(TestPlayer player)
	{
		if (player.HasInputAuthority && Runner.IsForward)
			dashBoard = GameManager.UI.ShowSceneUI(dashBoardPrefab);
	}

	protected override void OnGetOff()
	{
		if (HasInputAuthority && Runner.IsForward)
		{
			GameManager.UI.CloseSceneUI(dashBoard);
			dashBoard = null;
		}
	}
}