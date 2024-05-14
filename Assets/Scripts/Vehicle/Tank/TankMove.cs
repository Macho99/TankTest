using UnityEngine;

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

	const string statUIPrefabPath = "UI/Vehicle/Tank/TankStatUI";

	protected override void Awake()
	{
		base.Awake();
		stateMachine.OverrideState(State.Park.ToString(), new TankPark(this));
	}

	protected override void InstantiateStatUI()
	{
		//base.InstantiateStatUI();
		statUI = GameManager.UI.ShowSceneUI<TankStatUI>(statUIPrefabPath);
	}

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
			leftRollers[i].Rotate(360f * leftRps * Time.deltaTime, 0f, 0f);
			rightRollers[i].Rotate(360f * rightRps * Time.deltaTime, 0f, 0f);
		}
	}

	protected override void Steering()
	{
		if (oilFilled == false) return;

		const float RPM_DIFF = 10f;
		float torqueAdder = 2000f * engineHpRatio;
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