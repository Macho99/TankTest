using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 클림브 시스템
/// 종류 : 사다리 , 벽, 
/// </summary>
public abstract class PlayerClimb
{
    public abstract bool ClimbCheck(NetworkInputData input);

    public abstract void ClimbStart();
    public abstract void ClimbEnd();



}
