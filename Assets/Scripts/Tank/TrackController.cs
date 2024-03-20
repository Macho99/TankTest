using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

[RequireComponent(typeof(BezierSpline))]
public class TrackController : MonoBehaviour
{
	[SerializeField] float totalLength;
	[SerializeField] float updateInterval;
	[SerializeField] Vector3[] points;
	[SerializeField] Vector3[] vertexs;
	[SerializeField] float[] vertexLengths;
	[SerializeField] GameObject trackPrefab;
	[SerializeField] int tensionIdx = 6;
	[SerializeField] int frontIdx = 4;
	[SerializeField] int backIdx = 8;
	[SerializeField] float minTensionY = -0.03482562f;
	[SerializeField] float maxTensionY = 0.3694584f;
	[SerializeField] float maxUnderDiff = 0.2f;
	[SerializeField] int trackNum = 100;

	WheelCollider[] wheelCols;
	float baseUnderLength;
	BezierSpline spline;
	Tank tank;
	float nextUpdateTime;

	private void Awake()
	{
		tank = GetComponentInParent<Tank>();
		wheelCols = tank.LeftWheelCols;

		spline = GetComponent<BezierSpline>();
		points = new Vector3[spline.ControlPointCount];
		vertexs = new Vector3[spline.CurveCount];
		vertexLengths = new float[spline.CurveCount];
		Update();

		baseUnderLength = GetLength(backIdx, frontIdx, 20);
	}

	private void Start()
	{
		//CreateTracks();
		_ = StartCoroutine(CoTrackWalker());
	}

	private void Update()
	{
		//if (Time.time < nextUpdateTime) return;

		//nextUpdateTime = Time.time + updateInterval;

		int vertexIdx = frontIdx - 1;
		for (int i = 0; i < wheelCols.Length; i++)
		{
			wheelCols[i].GetWorldPose(out Vector3 pos, out Quaternion quat);
			pos.y -= wheelCols[i].radius;
			Vector3 trackPos = transform.InverseTransformPoint(pos);
			spline.SetControlPoint(vertexIdx * 3, trackPos);
			vertexIdx--;
			if (vertexIdx < 0)
			{
				vertexIdx = spline.CurveCount - 1;
			}
		}

		for (int i = 0; i < spline.ControlPointCount; i++)
		{
			points[i] = spline.GetControlPoint(i);
		}

		for (int i = 0; i < spline.CurveCount; i++)
		{
			vertexs[i] = spline.GetControlPoint(i * 3);
		}

		float underDiff = GetLength(backIdx, frontIdx, 20) - baseUnderLength;
		float ratio = underDiff / maxUnderDiff;
		ratio = Mathf.Clamp01(ratio);

		Vector3 newTensionPos = vertexs[tensionIdx];
		newTensionPos.y = Mathf.Lerp(minTensionY, maxTensionY, ratio);
		spline.SetControlPoint(tensionIdx * 3, newTensionPos);
		RefreshLengths(40);
	}

	private IEnumerator CoTrackWalker()
	{
		float distRatio = 0f;
		Transform trackWalker = Instantiate(trackPrefab).transform;

		while (true)
		{
			yield return null;

			distRatio += Time.deltaTime * 0.1f;
			if (distRatio > 1f)
				distRatio -= 1f;

			float t = GetTFromDistRatio(distRatio);

			Vector3 pos = spline.GetPoint(t);
			Vector3 dir = spline.GetDirection(t);
			Quaternion rot = Quaternion.LookRotation(dir, Vector3.Cross(dir, transform.forward));
			trackWalker.SetPositionAndRotation(pos, rot);
		}
	}

	float prevDist;
	private float GetTFromDistRatio(float distRatio)
	{
		distRatio = Mathf.Clamp01(distRatio);

		float t;
		float curDist = distRatio * totalLength;
		print(curDist - prevDist);
		prevDist = curDist;
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

		float tPerVertex = 1f / vertexLengths.Length;
		t = tPerVertex * i;
		t += tPerVertex * (curDist / vertexLengths[i]);
		return t;
	}

	private void RefreshLengths(int segments)
	{
		float totalLength = 0f;
		for(int startIdx = 0; startIdx < spline.CurveCount; startIdx++)
		{
			int endIdx = startIdx + 1;

			float length = 0f;
			float startT = 1f / spline.CurveCount * startIdx;
			float endT = 1f / spline.CurveCount * endIdx;
			Vector3 p0, p1;

			float deltaT = (endT - startT) / segments;
			for (int i = 0; i < segments; i++)
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

	private float GetLength(int startIdx, int endIdx, int segments)
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
			float deltaT = (endT - startT) / segments;
			for (int i = 0; i < segments; i++)
			{
				p0 = spline.GetPoint(startT + deltaT * i);
				p1 = spline.GetPoint(startT + deltaT * (i + 1));
				length += Vector3.Distance(p0, p1);
			}
		}
		else
		{
			float deltaT = (1f - startT + endT) / segments;
			int i;
			for (i = 0; startT + deltaT * i < 1f; i++)
			{
				p0 = spline.GetPoint(startT + deltaT * i);
				p1 = spline.GetPoint(startT + deltaT * (i + 1));
				length += Vector3.Distance(p0, p1);
			}
			int lastI = i;
			for (; i < segments; i++)
			{
				p0 = spline.GetPoint(deltaT * (i - lastI));
				p1 = spline.GetPoint(deltaT * (i - lastI + 1));
				length += Vector3.Distance(p0, p1);
			}
		}

		return length;
	}
	private void CreateTracks()
	{
		float stepSize = 1f / trackNum;
		for (int i = 0; i < trackNum; i++)
		{
			float t = GetTFromDistRatio(stepSize * i);
			Vector3 pos = spline.GetPoint(t);
			Vector3 dir = spline.GetDirection(t);
			Quaternion rot = Quaternion.LookRotation(dir, Vector3.Cross(dir, transform.forward));
			Instantiate(trackPrefab, pos, rot, transform);
		}
	}

}
