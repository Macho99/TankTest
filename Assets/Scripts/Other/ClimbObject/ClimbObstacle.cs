using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ClimbObstacle : ClimbObject, IClimbPassable
{
    private BoxCollider climbCollider;
    private float sizeY;
    private float offsetY;
    private void Awake()
    {
        climbCollider = GetComponent<BoxCollider>();
        sizeY = climbCollider.size.y;
        offsetY = 0.1f;
    }


    public bool CanClimbPassCheck(Vector3 position, float height)
    {
        //위에 플레이어가 통과할만한 공간이 있는지
        Vector3 origin = transform.position + climbCollider.center + Vector3.up * climbCollider.size.y / 2;
        Debug.DrawRay(origin, Vector3.up * height, Color.blue, 0.5f);
        if (Physics.Raycast(origin, Vector3.up, out RaycastHit hit, height))
        {
            Debug.Log(hit.collider.gameObject);


            return false;
        }
        //플레이어가 검사할때 플레이어의 위치가 장애물의 높이 차이가 collider의 heigt 랑 0.1차이날때?

        float distance = origin.y - position.y;
        if (distance < sizeY - offsetY && distance > sizeY + offsetY)
        {
            return false;
        }
               
        return true;
    }
}
