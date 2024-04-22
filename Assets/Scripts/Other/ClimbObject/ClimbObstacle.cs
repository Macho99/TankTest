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
        //���� �÷��̾ ����Ҹ��� ������ �ִ���
        Vector3 origin = transform.position + climbCollider.center + Vector3.up * climbCollider.size.y / 2;
        Debug.DrawRay(origin, Vector3.up * height, Color.blue, 0.5f);
        if (Physics.Raycast(origin, Vector3.up, out RaycastHit hit, height))
        {
            Debug.Log(hit.collider.gameObject);


            return false;
        }
        //�÷��̾ �˻��Ҷ� �÷��̾��� ��ġ�� ��ֹ��� ���� ���̰� collider�� heigt �� 0.1���̳���?

        float distance = origin.y - position.y;
        if (distance < sizeY - offsetY && distance > sizeY + offsetY)
        {
            return false;
        }
               
        return true;
    }
}
