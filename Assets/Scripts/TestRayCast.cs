using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRayCast : MonoBehaviour
{
    private void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 20f, Color.red);
    }

}
