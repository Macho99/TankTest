using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Ŭ���� �ý���
/// ���� : ��ٸ� , ��, 
/// </summary>
public abstract class PlayerClimb
{
    public abstract bool ClimbCheck(NetworkInputData input);

    public abstract void ClimbStart();
    public abstract void ClimbEnd();



}
