using UnityEngine;

public class BezierCurve : MonoBehaviour 
{

	public Vector3[] points;
	
	public Vector3 GetPoint (float t) 
	{
		return transform.TransformPoint(Bezier.GetPoint(points[0], points[1], points[2], points[3], t));
	}
	
	public Vector3 GetVelocity (float t) 
	{
		return transform.TransformPoint(Bezier.GetFirstDerivative(points[0], points[1], points[2], points[3], t)) - transform.position;
	}
	
	public Vector3 GetDirection (float t) 
	{
		return GetVelocity(t).normalized;
	}

	public float GetTotalLength(int nrOfSegments)
	{
		float totalLength = 0;
		float t = 1.0f/nrOfSegments;
		Vector3 p0,p1;

		for(int i=0; i<nrOfSegments; i++)
		{
			p0 = GetPoint( t*i );
			p1 = GetPoint( t*(i+1));
			totalLength += Vector3.Distance(p0,p1);
		}
		return totalLength;
	}
	
	public void Reset () 
	{
		points = new Vector3[] 
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f),
			new Vector3(3f, 0f, 0f),
			new Vector3(4f, 0f, 0f)
		};
	}
}