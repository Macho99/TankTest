using Fusion;
using Fusion.Addons.SimpleKCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 클림브 시스템
/// 종류 : 사다리 , 장애물 
/// </summary>
public class PlayerClimb : NetworkBehaviour
{
    private SimpleKCC kcc;
    private void Awake()
    {
        kcc = GetComponent<SimpleKCC>();
    }

    public bool ClimbCheck(ClimbType type)
    {
        Ray ray = new Ray();

        ray.origin = transform.position + transform.forward * kcc.Settings.Radius + Vector3.up * 0.3f;
        ray.direction = transform.forward;

        if (Physics.Raycast(ray, out RaycastHit hit, 1f))
        {
            if (hit.collider.TryGetComponent(out IClimbPassable climbObject))
            {
                if (climbObject.CanClimbPassCheck(hit, transform.position, kcc.Settings.Height))
                {
                    return true;
                }
            }
        }
        Debug.DrawRay(ray.origin, ray.direction * 1f, Color.red, 0.5f);
        return false;
    }
    public void TryClimb()
    {

    }
}
