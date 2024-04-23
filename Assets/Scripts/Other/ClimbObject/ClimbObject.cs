using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClimbType { Radder, Obstacle }

public abstract class ClimbObject : MonoBehaviour
{

    public ClimbType type { get; protected set; }
}
