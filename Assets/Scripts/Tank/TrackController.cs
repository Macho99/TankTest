using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

[RequireComponent(typeof(BezierSpline))]
public class TrackController : MonoBehaviour
{
	[Serializable]
	public enum TrackType { Left, Right };

	[SerializeField] TrackType trackType;
	[SerializeField] float totalLength;
	//[SerializeField] Vector3[] points;
	//[SerializeField] Vector3[] vertexs;
	[SerializeField] float[] vertexLengths;
	[SerializeField] GameObject trackPrefab;
	[SerializeField] int tensionIdx = 6;
	[SerializeField] int frontIdx = 4;
	[SerializeField] int backIdx = 8;
	[SerializeField] float minTensionY = -0.03482562f;
	[SerializeField] float maxTensionY = 0.3694584f;
	[SerializeField] float maxUnderDiff = 0.3f;
	[SerializeField] int trackNum = 78;
	[SerializeField] float controlMagMultiplier = 0.32f;
	public float Velocity { get; set; }

	Tank tank;
	Transform[] tracks;
	WheelCollider[] wheelCols;
	Transform[] wheelTrans;
	float baseUnderLength;
	BezierSpline spline;
	float curTrackOffset;

	private void Awake()
	{
		tank = GetComponentInParent<Tank>();
		if (trackType == TrackType.Left)
		{
			wheelCols = tank.LeftWheelCols;
			wheelTrans = tank.LeftWheelTrans;
		}
		else//(trackType == TrackType.Right)
		{
			wheelCols = tank.RightWheelCols;
			wheelTrans = tank.RightWheelTrans;
		}

		spline = GetComponent<BezierSpline>();
		//points = new Vector3[spline.ControlPointCount];
		//vertexs = new Vector3[spline.CurveCount];
		vertexLengths = new float[spline.CurveCount];
		Update();

		baseUnderLength = GetLength(backIdx, frontIdx, 20);

		InitSpline();
		CreateTracks();
	}


	private void Update()
	{
		//if (Time.time < nextUpdateTime) return;

		//nextUpdateTime = Time.time + updateInterval;

		int vertexIdx = frontIdx - 1;
		for (int i = 1; i < wheelCols.Length - 1; i++)
		{
			wheelCols[i].GetWorldPose(out Vector3 pos, out Quaternion quat);
			Vector3 trackPos = transform.InverseTransformPoint(pos);
			trackPos.y -= wheelCols[i].radius;
			spline.SetControlPoint(vertexIdx * 3, trackPos);
			vertexIdx--;
			if (vertexIdx < 0)
			{
				vertexIdx = spline.CurveCount - 1;
			}
		}

		//for (int i = 0; i < spline.ControlPointCount; i++)
		//{
		//	points[i] = spline.GetControlPoint(i);
		//}

		//for (int i = 0; i < spline.CurveCount; i++)
		//{
		//	vertexs[i] = spline.GetControlPoint(i * 3);
		//}

		AdjustTension();
		RefreshLengths(5);

		TrackPlace();
	}

	private void AdjustTension()
	{
		float underDiff = GetLength(backIdx, frontIdx, 20) - baseUnderLength;
		float ratio = underDiff / maxUnderDiff;
		ratio = Mathf.Clamp01(ratio);

		Vector3 newTensionPos = spline.GetControlPoint(tensionIdx * 3);
		newTensionPos.y = Mathf.Lerp(minTensionY, maxTensionY, ratio);
		spline.SetControlPoint(tensionIdx * 3, newTensionPos);

		Vector3 prevControl = spline.GetControlPoint(tensionIdx * 3 - 2);
		Vector3 prevVertex = spline.GetVertexPoint(tensionIdx - 1);
		float magnitude = (prevControl - prevVertex).magnitude;
		Vector3 prevDir = (newTensionPos - prevVertex).normalized;
		spline.SetControlPoint(tensionIdx * 3 - 2, prevVertex + prevDir * magnitude);

		//Vector3 nextControl = spline.GetControlPoint(tensionIdx * 3 + 2);
		Vector3 nextVertex = spline.GetVertexPoint(tensionIdx + 1);
		Vector3 nextDir = prevDir;
		nextDir.y = -nextDir.y;
		spline.SetControlPoint(tensionIdx * 3 + 2, nextVertex - nextDir * magnitude);
	}

	private void TrackPlace()
	{
		if (tracks == null) return;

		//if (Mathf.Approximately(speed, 0f) == true) return;
		curTrackOffset -= Velocity * 1000 / 3600 / totalLength * Time.deltaTime ;

		float distRatioStep = 1f / tracks.Length;
		for (int i = 0; i < tracks.Length; i++)
		{
			float distRatio = curTrackOffset + distRatioStep * i;
			distRatio = Mathf.Repeat(distRatio, 1f);

			float t = GetTFromDistRatio(distRatio);

			Vector3 pos = spline.GetPoint(t);
			Vector3 dir = spline.GetDirection(t);
			Quaternion rot = Quaternion.LookRotation(dir, Vector3.Cross(dir, transform.forward));
			tracks[i].SetPositionAndRotation(pos, rot);
		}
	}

	private float GetTFromDistRatio(float distRatio)
	{
		distRatio = Mathf.Clamp01(distRatio);

		float t;
		float curDist = distRatio * totalLength;
		int i;

		for(i = 0; i < vertexLengths.Length; i++)
		{
			if(curDist - vertexLengths[i] < 0f)
			{
				break;
			}
			else
			{
				curDist -= vertexLengths[i];
			}
		}

		// 예외상황일경우 한바퀴 더 돌리기(트랙 길이는 유동적)
		if(i == vertexLengths.Length)
		{
			for (i = 0; i < vertexLengths.Length; i++)
			{
				if (curDist - vertexLengths[i] < 0f)
				{
					break;
				}
				else
				{
					curDist -= vertexLengths[i];
				}
			}
		}

		float tPerVertex = 1f / vertexLengths.Length;
		t = tPerVertex * i;
		t += tPerVertex * (curDist / vertexLengths[i]);
		return t;
	}

	private void RefreshLengths(int segmentPerCurve)
	{
		float totalLength = 0f;
		for(int startIdx = 0; startIdx < spline.CurveCount; startIdx++)
		{
			int endIdx = startIdx + 1;

			float length = 0f;
			float startT = 1f / spline.CurveCount * startIdx;
			float endT = 1f / spline.CurveCount * endIdx;
			Vector3 p0, p1;

			float deltaT = (endT - startT) / segmentPerCurve;
			for (int i = 0; i < segmentPerCurve; i++)
			{
				p0 = spline.GetPoint(startT + deltaT * i);
				p1 = spline.GetPoint(startT + deltaT * (i + 1));
				length += Vector3.Distance(p0, p1);
			}
			vertexLengths[startIdx] = length;
			totalLength += length;
		}
		this.totalLength = totalLength;
	}

	private float GetLength(int startIdx, int endIdx, int segment)
	{
		if (startIdx < 0 || endIdx > spline.ControlPointCount / 3)
		{
			Debug.LogError($"start:{startIdx}, end:{endIdx} \n값이 올바른지 확인하세요");
			return -1f;
		}
		if (spline.Loop == false && startIdx > endIdx)
		{
			Debug.LogError($"루프모드가 아닐때는 start가 end보다 클 수 없습니다");
			return -1f;
		}
		if (startIdx == endIdx)
		{
			return 0f;
		}

		float length = 0f;
		float startT = 1f / spline.CurveCount * startIdx;
		float endT = 1f / spline.CurveCount * endIdx;

		Vector3 p0, p1;

		if (startIdx < endIdx)
		{
			float deltaT = (endT - startT) / segment;
			for (int i = 0; i < segment; i++)
			{
				p0 = spline.GetPoint(startT + deltaT * i);
				p1 = spline.GetPoint(startT + deltaT * (i + 1));
				length += Vector3.Distance(p0, p1);
			}
		}
		else
		{
			float deltaT = (1f - startT + endT) / segment;
			int i;
			for (i = 0; startT + deltaT * i < 1f; i++)
			{
				p0 = spline.GetPoint(startT + deltaT * i);
				p1 = spline.GetPoint(startT + deltaT * (i + 1));
				length += Vector3.Distance(p0, p1);
			}
			int lastI = i;
			for (; i < segment; i++)
			{
				p0 = spline.GetPoint(deltaT * (i - lastI));
				p1 = spline.GetPoint(deltaT * (i - lastI + 1));
				length += Vector3.Distance(p0, p1);
			}
		}

		return length;
	}

	private void InitSpline()
	{
		for (int i = 0; i < spline.CurveCount; i++)
		{
			float length = vertexLengths[i];
			float magnitude = length * controlMagMultiplier;
			Vector3 prevVertex = spline.GetVertexPoint(i);
			Vector3 nextVertex = spline.GetVertexPoint(i + 1);
			Vector3 prevControl = spline.GetControlPoint(i * 3 + 1);
			Vector3 nextControl = spline.GetControlPoint(i * 3 + 2);

			Vector3 prevDir = (prevControl - prevVertex).normalized;
			Vector3 nextDir = (nextControl - nextVertex).normalized;
			spline.SetControlPoint(i * 3 + 1, prevVertex + prevDir * magnitude);
			spline.SetControlPoint(i * 3 + 2, nextVertex + nextDir * magnitude);
		}
	}

	private void CreateTracks()
	{
		tracks = new Transform[trackNum];
		float stepSize = 1f / trackNum;
		for (int i = 0; i < trackNum; i++)
		{
			float t = GetTFromDistRatio(stepSize * i);
			Vector3 pos = spline.GetPoint(t);
			Vector3 dir = spline.GetDirection(t);
			Quaternion rot = Quaternion.LookRotation(dir, Vector3.Cross(dir, transform.forward));
			tracks[i] = Instantiate(trackPrefab, pos, rot, transform).transform;
		}
	}
}
