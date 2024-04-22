using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IClimbPassable
{
    public bool CanClimbPassCheck(Vector3 position, float height);
}
public struct ClimbPassData
{
    public float height;
    public Vector3 normal;

}