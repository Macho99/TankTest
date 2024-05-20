using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TrailerMove : NetworkBehaviour
{
	[SerializeField] Transform[] leftWheelTrans;
	[SerializeField] Transform[] rightWheelTrans;
	[SerializeField] WheelCollider[] leftWheelCols;
	[SerializeField] WheelCollider[] rightWheelCols;
	[SerializeField] float breakForce = 200000f;
	Joint joint;

	float prevLeftXAngle = 0f;
	float prevRightXAngle = 0f;
	[Networked] public float LeftAps { get; private set; } //Angle per second
	[Networked] public float RightAps { get; private set; }
	

	private void Awake()
	{
		joint = GetComponent<Joint>();
	}

	public override void Spawned()
	{
		for(int i = 0; i < leftWheelCols.Length; i++)
		{
			leftWheelCols[i].motorTorque = 0.000001f;
		}
		for (int i = 0; i < rightWheelCols.Length; i++)
		{
			rightWheelCols[i].motorTorque = 0.000001f;
		}
		joint.breakForce = breakForce;
		joint.breakTorque = breakForce;
	}

	public override void FixedUpdateNetwork()
	{
		if (Runner.IsForward)
		{
			leftWheelCols[0].GetWorldPose(out Vector3 rPos, out Quaternion rRot);
			Quaternion parentRot = leftWheelTrans[0].parent.rotation;
			Vector3 localEuler = (Quaternion.Inverse(parentRot) * rRot).eulerAngles;
			float curLeftXAngle = localEuler.z < 90f ? localEuler.x : localEuler.z - localEuler.x;
			LeftAps = (Mathf.DeltaAngle(prevLeftXAngle, curLeftXAngle)) / Runner.DeltaTime;
			prevLeftXAngle = curLeftXAngle;

			rightWheelCols[0].GetWorldPose(out Vector3 lPos, out Quaternion lRot);
			localEuler = (Quaternion.Inverse(parentRot) * lRot).eulerAngles;
			float curRightXAngle = localEuler.z < 90f ? localEuler.x : localEuler.z - localEuler.x;
			RightAps = (Mathf.DeltaAngle(prevRightXAngle, curRightXAngle)) / Runner.DeltaTime;
			prevRightXAngle = curRightXAngle;
		}
	}

	public override void Render()
	{
		for (int i = 0; i < leftWheelTrans.Length; i++)
		{
			if (Mathf.Abs(LeftAps) > 50f)
			{
				leftWheelTrans[i].Rotate(Vector3.right, LeftAps * Time.deltaTime);
			}
			leftWheelCols[i].GetWorldPose(out Vector3 pos, out Quaternion rot);
			leftWheelTrans[i].position = pos;
		}
		for (int i = 0; i < rightWheelTrans.Length; i++)
		{
			if (Mathf.Abs(RightAps) > 50f)
			{
				rightWheelTrans[i].Rotate(Vector3.right, RightAps * Time.deltaTime);
			}
			rightWheelCols[i].GetWorldPose(out Vector3 pos, out Quaternion rot);
			rightWheelTrans[i].position = pos;
		}
	}

	private void OnJointBreak(float breakForce)
	{
		joint.connectedBody.GetComponent<VehicleMove>()?.SetTrail(null);
		print($"트레일러 끊김: {breakForce}");
	}
}